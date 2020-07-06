using System;
using System.Threading.Tasks;
using ApiProxy.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ApiProxy.Services
{
    public class CheckAdminService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public CheckAdminService( IServiceProvider serviceProvider, ILogger<CheckAdminService> logger )
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        // Check if admin is exist
        private async Task<bool> CheckAdmin( IServiceScope serviceScope )
        {
            try
            {
                var _userManager = serviceScope.ServiceProvider.GetService<UserManager<ApiProxyUser>>();
                var txadmin = await _userManager.FindByEmailAsync( "admin@apiproxy.com" );
                if( txadmin == null )
                {
                    _logger.LogInformation( $"Admin Account Not Found, Add Admin Account admin@apiproxy.com" );
                    var user = new ApiProxyUser { UserName = "admin", Email = "admin@apiproxy.com" };
                    await _userManager.CreateAsync( user, "admin" );
                    user.EmailConfirmed = true;
                }

                return true;
            }
            catch( Exception ex)
            {
                _logger.LogError( $"CheckAdmin() ERROR : {ex.Message}" );
                return false;
            }
            
        }

        public async Task Execute()
        {
            _logger.LogInformation( "CheckAdmin Task Start" );
            using( var serviceScope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope() )
            {
                if( !await CheckAdmin( serviceScope ) )
                    _logger.LogDebug( "CheckAdmin Task has something wrong." );
            }
            _logger.LogInformation( "CheckAdmin Task End" );
        }
    }
}
