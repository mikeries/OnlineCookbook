using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CookBook.Startup))]
namespace CookBook
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
