using Microsoft.Extensions.Logging;
using Ombi.Api.Emby;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Serilog;

namespace Ombi.Schedule.Jobs.Emby
{
    public class EmbyContentCacher
    {
        public EmbyContentCacher(ISettingsService<EmbySettings> settings, IEmbyApi api, ILogger<EmbyContentCacher> logger)
        {

        }
        private bool ValidateSettings(EmbySettings emby)
        {
            if (emby.Enable)
            {
                foreach (var server in emby.Servers)
                {
                    if (server?.Ip == null || string.IsNullOrEmpty(server?.ApiKey))
                    {
                        //Log.Warn("A setting is null, Ensure Emby is configured correctly, and we have a Emby Auth token.");
                        return false;
                    }
                }
            }
            return emby.Enable;
        }

    }
}