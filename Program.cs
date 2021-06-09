using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;

namespace Modulr
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // We have to read it once before the services are loaded...
                    var port = 5001;
                    if (File.Exists("config.json"))
                    {
                        var configPortToken = JObject.Parse(File.ReadAllText("config.json"))
                            .GetValue("port", StringComparison.OrdinalIgnoreCase);
                        if (configPortToken != null)
                        {
                            var configPort = configPortToken.ToObject<int>();
                            if (configPort is >= 1 and <= 65535)
                                port = configPort;
                        }
                    }
                    webBuilder.UseStartup<Startup>().UseUrls($"https://*:{port}");
                });
    }
}