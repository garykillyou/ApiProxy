using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup( typeof( ApiProxy.Areas.Identity.IdentityHostingStartup ) )]
namespace ApiProxy.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure( IWebHostBuilder builder )
        {
            builder.ConfigureServices( ( context, services ) =>
            {
            } );
        }
    }
}