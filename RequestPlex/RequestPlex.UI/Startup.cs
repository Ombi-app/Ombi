using Owin;

using RequestPlex.Core;

namespace RequestPlex.UI
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseNancy();

            var s = new Setup();
            s.SetupDb();
        }
    }
}
