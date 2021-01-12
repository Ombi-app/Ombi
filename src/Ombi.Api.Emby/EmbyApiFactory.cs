using Ombi.Api;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using System.Threading.Tasks;

namespace Ombi.Api.Emby
{
    public class EmbyApiFactory : IEmbyApiFactory
    {
        private readonly ISettingsService<EmbySettings> _embySettings;
        private readonly IApi _api;

        // TODO, if we need to derive futher, need to rework
        public EmbyApiFactory(ISettingsService<EmbySettings> embySettings, IApi api)
        {
            _embySettings = embySettings;
            _api = api;
        }

        public async Task<IEmbyApi> CreateClient()
        {
            var settings = await _embySettings.GetSettingsAsync();
            return CreateClient(settings);
        }

        public IEmbyApi CreateClient(EmbySettings settings)
        {
            return new EmbyApi(_api);
        }
    }

    public interface IEmbyApiFactory
    {
        Task<IEmbyApi> CreateClient();
        IEmbyApi CreateClient(EmbySettings settings);
    }
}
