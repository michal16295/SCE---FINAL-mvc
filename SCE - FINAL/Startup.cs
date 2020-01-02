using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SCE___FINAL.Startup))]
namespace SCE___FINAL
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
