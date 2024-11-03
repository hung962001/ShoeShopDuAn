using Microsoft.Owin;
using Owin;
using StackExchange.Redis;
using System.Configuration;

[assembly: OwinStartupAttribute(typeof(ShoeShopDuAn.Startup))]
namespace ShoeShopDuAn
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
           
        }

    }

  
}
