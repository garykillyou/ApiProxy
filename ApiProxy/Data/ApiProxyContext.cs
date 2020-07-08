using System;
using ApiProxy.Areas.DB.Models;
using ApiProxy.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ApiProxy.Data
{
    public class ApiProxyContext : IdentityDbContext<ApiProxyUser>
    {
        public ApiProxyContext( DbContextOptions<ApiProxyContext> options )
            : base( options )
        { }

        public DbSet<AuthenticationTicket> AuthenticationTickets { get; set; }

        public DbSet<ApiKeyInfo> ApiKeyInfos { get; set; }

        public DbSet<UrlReference> UrlReferences { get; set; }

        public DbSet<ApiWithUrl> ApiWithUrls { get; set; }

        public DbSet<AskApiKey> AskApiKeys { get; set; }

        public DbSet<AskUrlReference> AskUrlReferences { get; set; }

        protected override void OnModelCreating( ModelBuilder builder )
        {
            base.OnModelCreating( builder );

            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            builder.Entity<ApiWithUrl>().HasKey( a => new { a.UserEmail, a.UrlReferenceID } );

            builder.Entity<ApiWithUrl>().HasOne( au => au.ApiKeyInfo ).WithMany( ai => ai.ApiWithUrls )
                .HasForeignKey( au => au.UserEmail );

            builder.Entity<ApiWithUrl>().HasOne( au => au.UrlReference ).WithMany( ur => ur.ApiWithUrls )
                .HasForeignKey( au => au.UrlReferenceID );
        }

        public static string CreateKey( int size = 14 )
        {
            string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            Random random = new Random();

            char[] chars = new char[size];
            for( int i = 0; i < size; i++ )
            {
                chars[i] = validChars[random.Next( 0, validChars.Length )];
            }
            string res = new string( chars );

            return res;
        }
    }
}
