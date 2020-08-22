using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using PartyMemberManager.Dal;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PartyMemberManager.LoggerProviders;

namespace PartyMemberManager
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string dataBaseType = Configuration["DataBaseType"];
                services.AddDbContext<PMContext>(
                    options => options.UseSqlServer(Configuration.GetConnectionString("SqlServer"))
                );
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                               .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o =>

                               {
                                   o.Cookie.Name = "_AdminTicketCookie";
                                   o.LoginPath = new PathString("/Account/Login");
                                   o.AccessDeniedPath = new PathString("/Account/Forbidden");
                                   o.LogoutPath = new PathString("/Account/Logout");
                                   o.ReturnUrlParameter = new PathString("/Home/Index");
                                   o.ExpireTimeSpan = new TimeSpan(8, 0, 0);
                               });
            services.AddControllersWithViews();
            services.AddSession();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            loggerFactory.AddProvider(new LoggerDatabaseProvider(serviceProvider));

            //app.UseHttpsRedirection();
            app.UseStatusCodePagesWithReExecute("/error/{0}");
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
            app.UseFastReport();
        }
    }
}
