using Nancy;
using Nancy.Extensions;
using Nancy.Responses;

namespace RequestPlex.UI.Modules
{
    public class IndexModule : NancyModule
    {
        public IndexModule()
        {
            Get["/"] = parameters => Context.GetRedirect("~/search");
            Get["/Index"] = parameters => Context.GetRedirect("~/search");
        }
    }
}