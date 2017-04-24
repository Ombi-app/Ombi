using System.Security.Principal;
using AutoMapper;
using Ombi.Api.TvMaze;
using Ombi.Core.Requests.Models;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;

namespace Ombi.Core.Engine
{
    public class TvSearchEngine : BaseMediaEngine
    {

        public TvSearchEngine(IPrincipal identity, IRequestService service, ITvMazeApi tvMaze, IMapper mapper, ISettingsService<PlexSettings> plexSettings, ISettingsService<EmbySettings> embySettings) 
            : base(identity, service)
        {
            TvMazeApi = tvMaze;
            Mapper = mapper;
            PlexSettings = plexSettings;
            EmbySettings = embySettings;
        }

        private ITvMazeApi TvMazeApi { get; }
        private IMapper Mapper { get; }
        private ISettingsService<PlexSettings> PlexSettings { get; }
        private ISettingsService<EmbySettings> EmbySettings { get; }

       
    }
}
