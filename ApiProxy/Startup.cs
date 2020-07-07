using System;
using ApiProxy.Areas.Identity.Data;
using ApiProxy.Data;
using ApiProxy.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ApiProxy
{
    public class Startup
    {
        public Startup( IConfiguration configuration )
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices( IServiceCollection services )
        {
            services.AddDbContext<ApiProxyContext>( options =>
                options.UseMySql( Configuration.GetConnectionString( "ApiProxyContextConnection" ) ) );

            services.AddIdentity<ApiProxyUser, IdentityRole>( opts =>
            {
                // 密碼規則
                opts.Password.RequiredLength = 4;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequireDigit = false;
                opts.Password.RequireUppercase = false;
                opts.Password.RequireLowercase = false;
            } )
            .AddEntityFrameworkStores<ApiProxyContext>()
            .AddDefaultTokenProviders()
            .AddDefaultUI();

            services.AddRazorPages().AddRazorRuntimeCompilation();

            // Add Db Migration tool
            services.AddTransient<DbMigrationService>();
            // Add Admin Check tool
            services.AddTransient<CheckAdminService>();

            services.AddHttpClient();
            services.AddSingleton<HttpClientHelpers>();

            services.ConfigureApplicationCookie( options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromDays( 14 );
                options.SlidingExpiration = true;
                options.SessionStore = new CustomTicketStore( services );
            } );

            services.AddMvc( options => options.SuppressAsyncSuffixInActionNames = false )
            .AddJsonOptions( opt => opt.JsonSerializerOptions.PropertyNamingPolicy = null );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure( IApplicationBuilder app, IWebHostEnvironment env )
        {
            // Auto DataBase Migration
            app.ApplicationServices.GetRequiredService<DbMigrationService>().Execute();
            // Check Admin Exist
            app.ApplicationServices.GetRequiredService<CheckAdminService>().Execute().Wait();

            if( env.IsDevelopment() )
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler( "/Error" );
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints( endpoints =>
             {
                 endpoints.MapControllerRoute( "default", "{area:exists}/{controller=Home}/{action=Index}/{id?}" );
                 endpoints.MapRazorPages();
             } );
        }
    }
}
