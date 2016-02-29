using Nancy;

using RequestPlex.Core;
using RequestPlex.Core.SettingModels;

namespace RequestPlex.UI.Modules
{
    public class RequestsModule : NancyModule
    {
        public RequestsModule(ISettingsService<RequestPlexSettings>  s)
        {
            Get["requests/"] = _ => "Hello!";
            Get["requests/test"] = _ =>
            {
                var se = new RequestPlexSettings
                {
                    PlexAuthToken = "abc",
                    Port = 2344,
                    UserAuthentication = false
                };
                s.SaveSettings(se);
                var a = s.GetSettings();
                return "Hi!";
            };

        }
    }
}