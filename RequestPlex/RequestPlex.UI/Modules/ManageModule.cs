using Nancy;

namespace RequestPlex.UI.Modules
{
    public class ManageModule : NancyModule
    {
        public ManageModule()
        {
            Get["manage/"] = _ => "Hello!";
        }
    }
}