using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Modulr.Tester
{
    public class ModulrConfig
    {
        [JsonProperty] public string JDKPath { get; private set; }
        [JsonProperty] public bool UseDocker { get; private set; }
        [JsonProperty] public string DockerPath { get; private set; }
        [JsonProperty] public string SaveLocation { get; private set; }
        [JsonProperty] public string SourceLocation { get; private set; }
        [JsonProperty] public string MySqlServer { get; private set; }
        [JsonProperty] public int MySqlPort { get; private set; }
        [JsonProperty] public string MySqlPassword { get; private set; }

        public string MySqlConnection =>
            $"server={MySqlServer};user=modulr;database=Modulr;port={MySqlPort};password={MySqlPassword}";

        [JsonProperty] public string GoogleClientKey { get; private set; }
        [JsonProperty] public string GoogleSecret { get; private set; }
        [JsonProperty] public string HostedDomain { get; private set; }

        [JsonIgnore] private readonly ILogger<ModulrConfig> _logger;

        public ModulrConfig(ILogger<ModulrConfig> logger, string file = "config.json", bool verify = true)
        {
            var json = File.ReadAllText(file);
            JsonConvert.PopulateObject(json, this);
            _logger = logger;
            if (verify)
                VerifyConfig();
        }

        private readonly string[] _dockerWinPath = {
            @"C:\Program Files\Docker\Docker\resources\docker.exe", 
            @"C:\Program Files\Docker\Docker\resources\bin\com.docker.cli.exe"
        };
        
        private readonly string[] _dockerLinPath = {
            @"/usr/bin/docker", 
            @"/usr/local/bin/docker"
        };

        private void VerifyConfig()
        {
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            if (UseDocker && (DockerPath == null || !File.Exists(DockerPath)))
            {
                var dockerInstalls = isWindows ? _dockerWinPath : _dockerLinPath;
                foreach (var path in dockerInstalls)
                    if (File.Exists(path))
                    {
                        DockerPath = path;
                        break;
                    }
            }
            _logger.LogInformation($"Found Docker install at {DockerPath}");
        }
    }
}