using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ombi.Api.Lidarr;
using Ombi.Api.Lidarr.Models;
using Ombi.Api.MusicBrainz;
using Ombi.Core.Authentication;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search.V2.Music;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Artist = Hqub.MusicBrainz.API.Entities.Artist;
using ReleaseGroup = Ombi.Core.Models.Search.V2.Music.ReleaseGroup;

namespace Ombi.Core.Engine.V2
{
    public class MusicSearchEngineV2 : BaseMediaEngine, IMusicSearchEngineV2
    {
        private readonly IMusicBrainzApi _musicBrainzApi;
        private readonly ISettingsService<LidarrSettings> _lidarrSettings;
        private readonly ILidarrApi _lidarrApi;

        public MusicSearchEngineV2(IPrincipal identity, IRequestServiceMain requestService, IRuleEvaluator rules,
            OmbiUserManager um, ICacheService cache, ISettingsService<OmbiSettings> ombiSettings,
            IRepository<RequestSubscription> sub, IMusicBrainzApi musicBrainzApi, ISettingsService<LidarrSettings> lidarrSettings,
            ILidarrApi lidarrApi)
            : base(identity, requestService, rules, um, cache, ombiSettings, sub)
        {
            _musicBrainzApi = musicBrainzApi;
            _lidarrSettings = lidarrSettings;
            _lidarrApi = lidarrApi;
        }

        public async Task<ReleaseGroup> GetAlbum(string albumId)
        {
            var g = await _musicBrainzApi.GetAlbumInformation(albumId);
            var release = new ReleaseGroup
            {
                ReleaseType = g.ReleaseGroup.PrimaryType,
                Id = g.Id,
                Title = g.Title,
                ReleaseDate = g.ReleaseGroup.FirstReleaseDate,
            };

            await RunSearchRules(release);
            return release;
        }

        public async Task<ArtistInformation> GetArtistInformation(string artistId)
        {
            var artist = await _musicBrainzApi.GetArtistInformation(artistId);
            var lidarrSettings = await GetLidarrSettings();
            Task<ArtistResult> lidarrArtistTask = null;
            if (lidarrSettings.Enabled)
            {
                lidarrArtistTask = _lidarrApi.GetArtistByForeignId(artistId, lidarrSettings.ApiKey, lidarrSettings.FullUri);
            }

            var info = new ArtistInformation
            {
                Id = artistId,
                Name = artist.Name,
                Country = artist.Country,
                Region = artist.Area?.Name,
                Type = artist.Type,
                StartYear = artist.LifeSpan?.Begin ?? "",
                EndYear = artist.LifeSpan?.End ?? "",
                Disambiguation = artist.Disambiguation,
                ReleaseGroups = new List<ReleaseGroup>(),
                Members = new List<BandMember>()
            };

            foreach (var g in artist.ReleaseGroups)
            {
                var release = new ReleaseGroup
                {
                    ReleaseType = g.PrimaryType,
                    Id = g.Id,
                    Title = g.Title,
                    ReleaseDate = g.FirstReleaseDate,
                };

                await RunSearchRules(release);
                info.ReleaseGroups.Add(release);
            }

            info.Links = GetLinksForArtist(artist);
            info.Members = GetBandMembers(artist);

            if (lidarrArtistTask != null)
            {
                try
                {
                    var artistResult = await lidarrArtistTask;
                    info.Banner = artistResult.images?.FirstOrDefault(x => x.coverType.Equals("banner", StringComparison.InvariantCultureIgnoreCase))?.url.ToHttpsUrl();
                    info.Logo = artistResult.images?.FirstOrDefault(x => x.coverType.Equals("logo", StringComparison.InvariantCultureIgnoreCase))?.url.ToHttpsUrl();
                    info.Poster = artistResult.images?.FirstOrDefault(x => x.coverType.Equals("poster", StringComparison.InvariantCultureIgnoreCase))?.url.ToHttpsUrl();
                    info.FanArt = artistResult.images?.FirstOrDefault(x => x.coverType.Equals("fanart", StringComparison.InvariantCultureIgnoreCase))?.url.ToHttpsUrl();
                    info.Overview = artistResult.overview;
                }
                catch (JsonSerializationException)
                {
                    // swallow, Lidarr probably doesn't have this artist
                }
            }

            return info;
        }

        public async Task<AlbumArt> GetReleaseGroupArt(string musicBrainzId, CancellationToken token)
        {
            var art = await _musicBrainzApi.GetCoverArtForReleaseGroup(musicBrainzId, token);

            if (art == null || !art.images.Any())
            {
                return new AlbumArt();
            }

            foreach (var cover in art.images)
            {
                if ((cover.thumbnails?.small ?? string.Empty).HasValue())
                {
                    return new AlbumArt(cover.thumbnails.small.ToHttpsUrl());
                }
                if ((cover.thumbnails?.large ?? string.Empty).HasValue())
                {
                    return new AlbumArt(cover.thumbnails.large.ToHttpsUrl());
                }
            }

            return new AlbumArt();
        }

        public async Task<ArtistInformation> GetArtistInformationByRequestId(int requestId)
        {
            var request = await RequestService.MusicRequestRepository.Find(requestId);
            return await GetArtistInformation(request.ForeignArtistId);
        }

        private List<BandMember> GetBandMembers(Artist artist)
        {
            var members = new List<BandMember>();
            var membersOfBand = artist.Relations.Where(x => x.TypeId == RelationLinks.BandMember);
            foreach (var member in membersOfBand)
            {
                members.Add(new BandMember
                {
                    Name = member.Artist?.Name,
                    Attributes = member.Attributes,
                    IsCurrentMember = member.Ended == null,
                    End = member.End,
                    Start = member.Begin
                });
            }

            return members;
        }

        private ArtistLinks GetLinksForArtist(Artist artist)
        {
            var links = new ArtistLinks();
            foreach (var relation in artist.Relations)
            {
                switch (relation.TypeId)
                {
                    case RelationLinks.AllMusic:
                        links.AllMusic = relation.Url?.Resource.ToHttpsUrl();
                        break;
                    case RelationLinks.BbcMusic:
                        links.BbcMusic = relation.Url?.Resource.ToHttpsUrl();
                        break;
                    case RelationLinks.Discogs:
                        links.Discogs = relation.Url?.Resource.ToHttpsUrl();
                        break;
                    case RelationLinks.Homepage:
                        links.HomePage = relation.Url?.Resource.ToHttpsUrl();
                        break;
                    case RelationLinks.Imdb:
                        links.Imdb = relation.Url?.Resource.ToHttpsUrl();
                        break;
                    case RelationLinks.LastFm:
                        links.LastFm = relation.Url?.Resource.ToHttpsUrl();
                        break;
                    case RelationLinks.MySpace:
                        links.MySpace = relation.Url?.Resource.ToHttpsUrl();
                        break;
                    case RelationLinks.OnlineCommunity:
                        links.OnlineCommunity = relation.Url?.Resource.ToHttpsUrl();
                        break;
                    case RelationLinks.SocialNetwork:
                        if ((relation.Url?.Resource ?? string.Empty).Contains("twitter", CompareOptions.IgnoreCase))
                        {
                            links.Twitter = relation.Url?.Resource.ToHttpsUrl();
                        }
                        if ((relation.Url?.Resource ?? string.Empty).Contains("facebook", CompareOptions.IgnoreCase))
                        {
                            links.Facebook = relation.Url?.Resource.ToHttpsUrl();
                        }
                        if ((relation.Url?.Resource ?? string.Empty).Contains("instagram", CompareOptions.IgnoreCase))
                        {
                            links.Instagram = relation.Url?.Resource.ToHttpsUrl();
                        }
                        if ((relation.Url?.Resource ?? string.Empty).Contains("vk", CompareOptions.IgnoreCase))
                        {
                            links.Vk = relation.Url?.Resource.ToHttpsUrl();
                        }
                        break;
                    case RelationLinks.Streams:
                        if ((relation.Url?.Resource ?? string.Empty).Contains("spotify", CompareOptions.IgnoreCase))
                        {
                            links.Spotify = relation.Url?.Resource.ToHttpsUrl();
                        }
                        if ((relation.Url?.Resource ?? string.Empty).Contains("deezer", CompareOptions.IgnoreCase))
                        {
                            links.Deezer = relation.Url?.Resource.ToHttpsUrl();
                        }

                        break;
                    case RelationLinks.YouTube:
                        links.YouTube = relation.Url?.Resource.ToHttpsUrl();
                        break;
                    case RelationLinks.Download:
                        if ((relation.Url?.Resource ?? string.Empty).Contains("google", CompareOptions.IgnoreCase))
                        {
                            links.Google = relation.Url?.Resource.ToHttpsUrl();
                        }
                        if ((relation.Url?.Resource ?? string.Empty).Contains("apple", CompareOptions.IgnoreCase))
                        {
                            links.Apple = relation.Url?.Resource.ToHttpsUrl();
                        }

                        break;
                }
            }

            return links;
        }

        private LidarrSettings __lidarrSettings;
        private async Task<LidarrSettings> GetLidarrSettings()
        {
            return __lidarrSettings ?? (__lidarrSettings = await _lidarrSettings.GetSettingsAsync());
        }
    }
}