using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ApiProxy.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ApiProxy.Services
{
    public class CustomTicketStore : ITicketStore
    {
        private readonly IServiceCollection _services;

        public CustomTicketStore( IServiceCollection services )
        {
            _services = services;
        }

        public async Task RemoveAsync( string key )
        {
            using( var scope = _services.BuildServiceProvider().CreateScope() )
            {
                var context = scope.ServiceProvider.GetService<ApiProxyContext>();
                if( Guid.TryParse( key, out var id ) )
                {
                    var ticket = await context.AuthenticationTickets.SingleOrDefaultAsync( x => x.Id == id );
                    if( ticket != null )
                    {
                        context.AuthenticationTickets.Remove( ticket );
                        await context.SaveChangesAsync();
                    }
                }
            }
        }

        public async Task RenewAsync( string key, AuthenticationTicket ticket )
        {
            using( var scope = _services.BuildServiceProvider().CreateScope() )
            {
                var context = scope.ServiceProvider.GetService<ApiProxyContext>();
                if( Guid.TryParse( key, out var id ) )
                {
                    var authenticationTicket = await context.AuthenticationTickets.FindAsync( id );
                    if( authenticationTicket != null )
                    {
                        authenticationTicket.Value = SerializeToBytes( ticket );
                        authenticationTicket.LastActivity = DateTimeOffset.UtcNow;
                        authenticationTicket.Expires = ticket.Properties.ExpiresUtc;
                        await context.SaveChangesAsync();
                    }
                }
            }
        }

        public async Task<AuthenticationTicket> RetrieveAsync( string key )
        {
            using( var scope = _services.BuildServiceProvider().CreateScope() )
            {
                var context = scope.ServiceProvider.GetService<ApiProxyContext>();
                if( Guid.TryParse( key, out var id ) )
                {
                    var authenticationTicket = await context.AuthenticationTickets.FindAsync( id );
                    if( authenticationTicket != null )
                    {
                        authenticationTicket.LastActivity = DateTimeOffset.UtcNow;
                        await context.SaveChangesAsync();

                        return DeserializeFromBytes( authenticationTicket.Value );
                    }
                }
            }

            return null;
        }

        public async Task<string> StoreAsync( AuthenticationTicket ticket )
        {
            using( var scope = _services.BuildServiceProvider().CreateScope() )
            {
                var userId = string.Empty;
                var nameIdentifier = ticket.Principal.FindFirstValue( ClaimTypes.NameIdentifier );
                var context = scope.ServiceProvider.GetService<ApiProxyContext>();

                if( ticket.AuthenticationScheme == "Identity.Application" )
                {
                    userId = nameIdentifier;
                }
                else if( ticket.AuthenticationScheme == "Identity.External" )
                {
                    userId = ( await context.UserLogins.SingleAsync( x => x.ProviderKey == nameIdentifier ) ).UserId;
                }

                var authenticationTicket = new Areas.Identity.Data.AuthenticationTicket()
                {
                    UserId = userId,
                    LastActivity = DateTimeOffset.UtcNow,
                    Value = SerializeToBytes( ticket )
                };

                var expiresUtc = ticket.Properties.ExpiresUtc;
                if( expiresUtc.HasValue )
                {
                    authenticationTicket.Expires = expiresUtc.Value;
                }

                context.AuthenticationTickets.Add( authenticationTicket );
                await context.SaveChangesAsync();

                return authenticationTicket.Id.ToString();
            }
        }

        private byte[] SerializeToBytes( AuthenticationTicket source )
            => TicketSerializer.Default.Serialize( source );

        private AuthenticationTicket DeserializeFromBytes( byte[] source )
            => source == null ? null : TicketSerializer.Default.Deserialize( source );
    }
}
