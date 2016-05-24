using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ServerManager.Startup))]
namespace ServerManager
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
