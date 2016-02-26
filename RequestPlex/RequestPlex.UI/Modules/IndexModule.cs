using Nancy;

namespace RequestPlex.UI.Modules
{
    public class IndexModule : NancyModule
    {
        public IndexModule()
        {
            Get["/"] = parameters => View["Index"];
            Get["/Index"] = parameters => View["Index"];
        }
    }
}