using System;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Modulr.Controllers;
using Modulr.Hubs;
using Modulr.Hubs.Workers;
using Modulr.Tester;

namespace Modulr
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
            services.AddControllers();
            services.AddSingleton<ModulrConfig>();
            services.AddSingleton<JavaUtils>();
            services.AddScoped<MySqlQuery>();
            services.AddScoped<GoogleAuth>();

            var tempConfig = new ModulrConfig(null, verify: false);
            services.AddAuthentication(o =>
            {
                o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            }).AddCookie().AddGoogle(o =>
            {
                o.ClientId = tempConfig.GoogleClientKey;
                o.ClientSecret = tempConfig.GoogleSecret;
            });
            services.AddSignalR();
            services.AddHostedService<TestWorker>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseStatusCodePagesWithReExecute("/Error", "?code={0}");
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            
            var webSocketOptions = new WebSocketOptions() 
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                AllowedOrigins = { "https://modulr.williamle.com", "https://modulrdev.williamle.com" }
            };

            app.UseWebSockets(webSocketOptions);
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<TestQueryHub>("/koumakan");
            });
        }
    }
}