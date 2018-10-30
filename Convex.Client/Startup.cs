using Convex.Client.Hubs;
using Convex.Client.Models.Proxy;
using Convex.Client.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

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
            services.Configure<CookiePolicyOptions>(options => {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSignalR();

            services.AddSingleton<IrcService>();
            services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<IrcService>());
            services.AddSingleton<IIrcService>(provider => provider.GetRequiredService<IrcService>());

            services.AddSingleton<IrcHubMethodsProxy>();
            services.AddSingleton<IIrcHubMethodsProxy>(provider => provider.GetRequiredService<IrcHubMethodsProxy>());

            services.AddSingleton<IrcHubProxy>();
            services.AddSingleton<IIrcHubProxy>(provider => provider.GetRequiredService<IrcHubProxy>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            } else {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseSignalR(routes => { routes.MapHub<IrcHub>("/IrcHub"); });
            app.UseMvc();
        }
    }
}