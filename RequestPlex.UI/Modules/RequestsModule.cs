using Nancy;

namespace RequestPlex.UI.Modules
{
    public class RequestsModule : NancyModule
    {
        public RequestsModule()
        {
            Get["requests/"] = _ => "Hello!";
        }
    }
}