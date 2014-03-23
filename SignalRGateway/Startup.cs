using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SignalRGateway.Startup))]
namespace SignalRGateway
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
