using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Modulr.Tester;

namespace Modulr;

public static class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                var port = 5001;
                var configPort = new ModulrConfig(null, verify: false).Port;
                if (configPort is >= 1 and <= 65535)
                    port = configPort;
                webBuilder.UseStartup<Startup>().UseUrls($"https://*:{port}");
            });
}