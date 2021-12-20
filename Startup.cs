using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Modulr.Controllers;
using Modulr.Controllers.Auth;
using Modulr.Hubs;
using Modulr.Hubs.Workers;
using Modulr.Tester;

namespace Modulr;

public class Startup
{
    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddSingleton<ModulrConfig>();
        services.AddSingleton<JavaUtils>();
        services.AddScoped<SqlQuery>();
        services.AddScoped<PasswordManager>();
        services.AddScoped<GoogleAuth>();
        services.AddHttpClient<Captcha>();

        services.AddAuthentication(o =>
        {
            o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        }).AddCookie(o =>
        {
            o.LoginPath = "/?error=Login%20is%20required%20to%20be%20on%20that%20page!";
        });

        services.AddSignalR();
        services.AddSingleton<TestWorker>();
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

        var config = new ModulrConfig(null, verify: false);
            
        var webSocketOptions = new WebSocketOptions
        {
            KeepAliveInterval = TimeSpan.FromSeconds(120)
        };
            
        foreach (var configWebSocketDomain in config.WebSocketDomains)
        {
            webSocketOptions.AllowedOrigins.Add(configWebSocketDomain);
        }

        app.UseWebSockets(webSocketOptions);
            
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<TestQueryHub>("/koumakan");
        });
            
        var cookiePolicyOptions = new CookiePolicyOptions
        {
            MinimumSameSitePolicy = SameSiteMode.Strict,
            Secure = CookieSecurePolicy.Always
        };

        app.UseCookiePolicy(cookiePolicyOptions);
    }
}