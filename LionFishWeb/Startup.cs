using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(LionFishWeb.Startup))]
namespace LionFishWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
            ConfigureAuth(app);
        }
    }
}
