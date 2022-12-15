using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using static webUi.Program;

namespace webUi
{
    public class Startup
    {
        private IConfiguration Configuration;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSession();
            services.AddControllersWithViews();                       
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseFileServer(new FileServerOptions()
            {
                FileProvider = new PhysicalFileProvider(
                Path.Combine(Directory.GetCurrentDirectory(), @"node_modules")),
                RequestPath = new PathString("/content"),
                EnableDirectoryBrowsing = true
            });
            app.UseSession();
            app.UseStaticFiles();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }       
            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: default,
                    pattern: "{controller=home}/{action=index}"
                );
                endpoints.MapControllerRoute(
                    name: default,
                    pattern: "home/productdetails/{id}",
                    defaults: new { controller = "home", action = "productdetails" }
                );
                endpoints.MapControllerRoute(
                    name: default,
                    pattern: "panel/editproduct/{id}",
                    defaults: new { controller = "panel", action = "editproduct" }
                );
                endpoints.MapControllerRoute(
                    name: default,
                    pattern: "panel/removeproduct/{id}",
                    defaults: new { controller = "panel", action = "removeproduct" }
                );
                endpoints.MapControllerRoute(
                    name: default,
                    pattern: "admin/updateprofile/{id}",
                    defaults: new { controller = "admin", action = "UpdateProfile" }
                );
                endpoints.MapControllerRoute(
                    name: default,
                    pattern: "admin/deleteuser/{id}",
                    defaults: new { controller = "admin", action = "deleteuser" }
                );
                
            });
        }
    }
}
