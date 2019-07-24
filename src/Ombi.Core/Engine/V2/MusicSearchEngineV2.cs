using System.Security.Principal;
using System.Threading.Tasks;
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
            
            return new ArtistInformation{
                Id = artistId,
                Name = artist.name,
                Country = artist.country,
                Region = artist.begin_area?.name,
                Type = artist.type,
                StartYear = artist.lifespan?.begin ?? "",
                EndYear = artist.lifespan?.end?? "",
            };
            
        } 
    }
}