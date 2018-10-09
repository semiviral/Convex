using Convex.Client.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Convex.Client {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        #region MEMBERS

        public IConfiguration Configuration { get; }

        #endregion

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddMvc();
            services.AddSingleton<IHostedService, ClientHostedService>(clientHostedService => new ClientHostedService("irc.foonetic.net", 6667));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes => {
                routes.MapRoute(
                    "default",
                    "{controller=Root}/{action=Index}/{id?}");
            });
        }
    }
}