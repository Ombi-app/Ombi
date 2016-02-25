using Nancy;

namespace RequestPlex.UI.Modules
{
    public class AdminModule : NancyModule
    {
        public AdminModule()
        {
            Get["admin/"] = _ => "Hello!";
        }
    }
}