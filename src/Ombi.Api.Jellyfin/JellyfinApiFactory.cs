using Ombi.Api;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using System.Threading.Tasks;

namespace Ombi.Api.Jellyfin
{
    public class JellyfinApiFactory : IJellyfinApiFactory
    {
        private readonly ISettingsService<JellyfinSettings> _jellyfinSettings;
        private readonly IApi _api;

        // TODO, if we need to derive futher, need to rework
        public JellyfinApiFactory(ISettingsService<JellyfinSettings> jellyfinSettings, IApi api)
        {
            _jellyfinSettings = jellyfinSettings;
            _api = api;
        }

        public async Task<IJellyfinApi> CreateClient()
        {
            var settings = await _jellyfinSettings.GetSettingsAsync();
            return CreateClient(settings);
        }

        public IJellyfinApi CreateClient(JellyfinSettings settings)
        {
            return new JellyfinApi(_api);
        }
    }

    public interface IJellyfinApiFactory
    {
        Task<IJellyfinApi> CreateClient();
        IJellyfinApi CreateClient(JellyfinSettings settings);
    }
}
