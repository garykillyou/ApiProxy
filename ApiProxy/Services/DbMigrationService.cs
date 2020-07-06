using System;
using ApiProxy.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ApiProxy.Services
{
    public class DbMigrationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public DbMigrationService( IServiceProvider serviceProvider, ILogger<DbMigrationService> logger )
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public void Execute()
        {
            using var serviceScope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            _logger.LogInformation( "Db Migration Task Start" );
            using( var context = serviceScope.ServiceProvider.GetService<ApiProxyContext>() )
            {
                context.Database.Migrate();
            }
            _logger.LogInformation( "Db Migration Task End" );
        }
    }
}
