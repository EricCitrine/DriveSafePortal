using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Telematics.Startup))]
namespace Telematics
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
