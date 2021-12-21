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
using ReleaseGroup = Ombi.Core.Models.Search.V2.Music.ReleaseGroup;

namespace Ombi.Core.Engine.V2
{
    public class MusicSearchEngineV2 : BaseMediaEngine, IMusicSearchEngineV2
    {
        private readonly ISettingsService<LidarrSettings> _lidarrSettings;
        private readonly ILidarrApi _lidarrApi;

        public MusicSearchEngineV2(IPrincipal identity, IRequestServiceMain requestService, IRuleEvaluator rules,
            OmbiUserManager um, ICacheService cache, ISettingsService<OmbiSettings> ombiSettings,
            IRepository<RequestSubscription> sub, ISettingsService<LidarrSettings> lidarrSettings,
            ILidarrApi lidarrApi)
            : base(identity, requestService, rules, um, cache, ombiSettings, sub)
        {
            _lidarrSettings = lidarrSettings;
            _lidarrApi = lidarrApi;
        }

        public async Task<ReleaseGroup> GetAlbum(string albumId)
        {
            var lidarrSettings = await GetLidarrSettings();
            Task<AlbumLookup> lidarrAlbumTask = null;
            var release = new ReleaseGroup{};
            if (lidarrSettings.Enabled)
            {
                lidarrAlbumTask = _lidarrApi.GetAlbumByForeignId(albumId, lidarrSettings.ApiKey, lidarrSettings.FullUri);
                var albumResult = await lidarrAlbumTask;
                release = new ReleaseGroup
                {
                    ReleaseType = albumResult.artistType,
                    Id = albumResult.artistId.ToString(),
                    Title = albumResult.title,
                    ReleaseDate = albumResult.releaseDate.ToString(),
                };

                await RunSearchRules(release);
            }
            
            return release;
        }

        public async Task<ArtistInformation> GetArtistInformation(string artistId)
        {
            var lidarrSettings = await GetLidarrSettings();
            Task<ArtistResult> lidarrArtistTask = null;
            var info = new ArtistInformation { };
            if (lidarrSettings.Enabled)
            {
                lidarrArtistTask = _lidarrApi.GetArtistByForeignId(artistId, lidarrSettings.ApiKey, lidarrSettings.FullUri);
                info = new ArtistInformation { };

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
                        info.Name = artistResult.artistName;
                        info.Monitored = artistResult.monitored;
                    }
                    catch (JsonSerializationException)
                    {
                        // swallow, Lidarr probably doesn't have this artist
                    }
                }
            }

            

            return info;
        }

        public async Task<AlbumInformation> GetAlbumInformation(string albumId)
        {
            var lidarrSettings = await GetLidarrSettings();
            Task<AlbumLookup> lidarrAlbumTask = null;
            var info = new AlbumInformation { };
            if (lidarrSettings.Enabled)
            {
                lidarrAlbumTask = _lidarrApi.GetAlbumByForeignId(albumId, lidarrSettings.ApiKey, lidarrSettings.FullUri);
            
                if (lidarrAlbumTask != null)
                {
                    try
                    {
                        var albumResult = await lidarrAlbumTask;
                        info.Cover = albumResult.images?.FirstOrDefault(x => x.coverType.Equals("cover", StringComparison.InvariantCultureIgnoreCase))?.url.ToHttpsUrl();
                        info.Title = albumResult.title;
                        info.Disambiguation = albumResult.disambiguation;
                        info.Overview = albumResult.overview;
                        info.Monitored = albumResult.monitored;
                        info.Id = albumResult.foreignAlbumId;
                    }
                    catch (JsonSerializationException)
                    {
                        // swallow, Lidarr probably doesn't have this album
                    }
                }
            }

            return info;
        }

        public async Task<AlbumArt> GetReleaseGroupArt(string musicBrainzId, CancellationToken token)
        {
            // var art = await _musicBrainzApi.GetCoverArtForReleaseGroup(musicBrainzId, token);

            // if (art == null || !art.images.Any())
            // {
            //     return new AlbumArt();
            // }

            // foreach (var cover in art.images)
            // {
            //     if ((cover.thumbnails?.small ?? string.Empty).HasValue())
            //     {
            //         return new AlbumArt(cover.thumbnails.small.ToHttpsUrl());
            //     }
            //     if ((cover.thumbnails?.large ?? string.Empty).HasValue())
            //     {
            //         return new AlbumArt(cover.thumbnails.large.ToHttpsUrl());
            //     }
            // }

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
            // var membersOfBand = artist.Relations.Where(x => x.TypeId == RelationLinks.BandMember);
            // foreach (var member in membersOfBand)
            // {
            //     members.Add(new BandMember
            //     {
            //         Name = member.Artist?.Name,
            //         Attributes = member.Attributes,
            //         IsCurrentMember = member.Ended == null,
            //         End = member.End,
            //         Start = member.Begin
            //     });
            // }

            return members;
        }

        private ArtistLinks GetLinksForArtist(Artist artist)
        {
            var links = new ArtistLinks();
            // foreach (var relation in artist.Relations)
            // {
            //     switch (relation.TypeId)
            //     {
            //         case RelationLinks.AllMusic:
            //             links.AllMusic = relation.Url?.Resource.ToHttpsUrl();
            //             break;
            //         case RelationLinks.BbcMusic:
            //             links.BbcMusic = relation.Url?.Resource.ToHttpsUrl();
            //             break;
            //         case RelationLinks.Discogs:
            //             links.Discogs = relation.Url?.Resource.ToHttpsUrl();
            //             break;
            //         case RelationLinks.Homepage:
            //             links.HomePage = relation.Url?.Resource.ToHttpsUrl();
            //             break;
            //         case RelationLinks.Imdb:
            //             links.Imdb = relation.Url?.Resource.ToHttpsUrl();
            //             break;
            //         case RelationLinks.LastFm:
            //             links.LastFm = relation.Url?.Resource.ToHttpsUrl();
            //             break;
            //         case RelationLinks.MySpace:
            //             links.MySpace = relation.Url?.Resource.ToHttpsUrl();
            //             break;
            //         case RelationLinks.OnlineCommunity:
            //             links.OnlineCommunity = relation.Url?.Resource.ToHttpsUrl();
            //             break;
            //         case RelationLinks.SocialNetwork:
            //             if ((relation.Url?.Resource ?? string.Empty).Contains("twitter", CompareOptions.IgnoreCase))
            //             {
            //                 links.Twitter = relation.Url?.Resource.ToHttpsUrl();
            //             }
            //             if ((relation.Url?.Resource ?? string.Empty).Contains("facebook", CompareOptions.IgnoreCase))
            //             {
            //                 links.Facebook = relation.Url?.Resource.ToHttpsUrl();
            //             }
            //             if ((relation.Url?.Resource ?? string.Empty).Contains("instagram", CompareOptions.IgnoreCase))
            //             {
            //                 links.Instagram = relation.Url?.Resource.ToHttpsUrl();
            //             }
            //             if ((relation.Url?.Resource ?? string.Empty).Contains("vk", CompareOptions.IgnoreCase))
            //             {
            //                 links.Vk = relation.Url?.Resource.ToHttpsUrl();
            //             }
            //             break;
            //         case RelationLinks.Streams:
            //             if ((relation.Url?.Resource ?? string.Empty).Contains("spotify", CompareOptions.IgnoreCase))
            //             {
            //                 links.Spotify = relation.Url?.Resource.ToHttpsUrl();
            //             }
            //             if ((relation.Url?.Resource ?? string.Empty).Contains("deezer", CompareOptions.IgnoreCase))
            //             {
            //                 links.Deezer = relation.Url?.Resource.ToHttpsUrl();
            //             }

            //             break;
            //         case RelationLinks.YouTube:
            //             links.YouTube = relation.Url?.Resource.ToHttpsUrl();
            //             break;
            //         case RelationLinks.Download:
            //             if ((relation.Url?.Resource ?? string.Empty).Contains("google", CompareOptions.IgnoreCase))
            //             {
            //                 links.Google = relation.Url?.Resource.ToHttpsUrl();
            //             }
            //             if ((relation.Url?.Resource ?? string.Empty).Contains("apple", CompareOptions.IgnoreCase))
            //             {
            //                 links.Apple = relation.Url?.Resource.ToHttpsUrl();
            //             }

            //             break;
            //     }
            // }

            return links;
        }

        private LidarrSettings __lidarrSettings;
        private async Task<LidarrSettings> GetLidarrSettings()
        {
            return __lidarrSettings ?? (__lidarrSettings = await _lidarrSettings.GetSettingsAsync());
        }
    }
}