using Nancy;

namespace RequestPlex.UI.Modules
{
    public class LoginModule : NancyModule
    {
        public LoginModule()
        {
            Get["/"] = _ => View["Login/Index"];
        }
    }
}