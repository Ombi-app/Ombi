using System;

using Owin;

namespace RequestPlex.UI
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            try
            {
                app.UseNancy();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                throw;
            }

        }
    }
}
