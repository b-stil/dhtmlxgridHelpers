using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DHXHelperDemo.Startup))]
namespace DHXHelperDemo
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
