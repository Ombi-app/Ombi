using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Hqub.MusicBrainz.API.Entities;
using Ombi.Api.MusicBrainz;
using Ombi.Core.Authentication;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search.V2.Music;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using ReleaseGroup = Ombi.Core.Models.Search.V2.Music.ReleaseGroup;

namespace Ombi.Core.Engine.V2
{
    public class MusicSearchEngineV2 : BaseMediaEngine, IMusicSearchEngineV2
    {
        private readonly IMusicBrainzApi _musicBrainzApi;

        public MusicSearchEngineV2(IPrincipal identity, IRequestServiceMain requestService, IRuleEvaluator rules,
            OmbiUserManager um, ICacheService cache, ISettingsService<OmbiSettings> ombiSettings,
            IRepository<RequestSubscription> sub, IMusicBrainzApi musicBrainzApi)
            : base(identity, requestService, rules, um, cache, ombiSettings, sub)
        {
            _musicBrainzApi = musicBrainzApi;
        }

        public async Task<ArtistInformation> GetArtistInformation(string artistId)
        {
            var artist = await _musicBrainzApi.GetArtistInformation(artistId);

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
            // TODO FINISH MAPPING
            foreach (var g in artist.ReleaseGroups)
            {
                info.ReleaseGroups.Add(new ReleaseGroup
                {
                    Type = g.PrimaryType,
                    Id = g.Id,
                    Title = g.Title,
                    ReleaseDate = g.FirstReleaseDate,
                });
            }

            info.Links = GetLinksForArtist(artist);
            info.Members = GetBandMembers(artist);
            return info;
        }

        private List<BandMember> GetBandMembers(Artist artist)
        {
            var members = new List<BandMember>();
            var membersOfBand = artist.Relations.Where(x => x.TypeId == "5be4c609-9afa-4ea0-910b-12ffb71e3821");
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
                        links.AllMusic = relation.Url?.Resource;
                        break;
                    case RelationLinks.BbcMusic:
                        links.BbcMusic = relation.Url?.Resource;
                        break;
                    case RelationLinks.Discogs:
                        links.Discogs = relation.Url?.Resource;
                        break;
                    case RelationLinks.Homepage:
                        links.HomePage = relation.Url?.Resource;
                        break;
                    case RelationLinks.Imdb:
                        links.Imdb = relation.Url?.Resource;
                        break;
                    case RelationLinks.LastFm:
                        links.LastFm = relation.Url?.Resource;
                        break;
                    case RelationLinks.MySpace:
                        links.MySpace = relation.Url?.Resource;
                        break;
                    case RelationLinks.OnlineCommunity:
                        links.OnlineCommunity = relation.Url?.Resource;
                        break;
                    case RelationLinks.SocialNetwork:
                        if ((relation.Url?.Resource ?? string.Empty).Contains("twitter", CompareOptions.IgnoreCase))
                        {
                            links.Twitter = relation.Url?.Resource;
                        }
                        if ((relation.Url?.Resource ?? string.Empty).Contains("facebook", CompareOptions.IgnoreCase))
                        {
                            links.Facebook = relation.Url?.Resource;
                        }
                        if ((relation.Url?.Resource ?? string.Empty).Contains("instagram", CompareOptions.IgnoreCase))
                        {
                            links.Instagram = relation.Url?.Resource;
                        }
                        if ((relation.Url?.Resource ?? string.Empty).Contains("vk", CompareOptions.IgnoreCase))
                        {
                            links.Vk = relation.Url?.Resource;
                        }
                        break;
                    case RelationLinks.Streams:
                        if ((relation.Url?.Resource ?? string.Empty).Contains("spotify", CompareOptions.IgnoreCase))
                        {
                            links.Spotify = relation.Url?.Resource;
                        }
                        if ((relation.Url?.Resource ?? string.Empty).Contains("deezer", CompareOptions.IgnoreCase))
                        {
                            links.Deezer = relation.Url?.Resource;
                        }

                        break;
                    case RelationLinks.YouTube:
                        links.YouTube = relation.Url?.Resource;
                        break;
                    case RelationLinks.Download:
                        if ((relation.Url?.Resource ?? string.Empty).Contains("google", CompareOptions.IgnoreCase))
                        {
                            links.Google = relation.Url?.Resource;
                        }
                        if ((relation.Url?.Resource ?? string.Empty).Contains("apple", CompareOptions.IgnoreCase))
                        {
                            links.Apple = relation.Url?.Resource;
                        }

                        break;
                }
            }

            return links;
        }
    }
}