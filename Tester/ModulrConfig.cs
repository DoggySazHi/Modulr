using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Modulr.Tester
{
    public class ModulrConfig
    {
        [JsonProperty] public string JDKPath { get; private set; }
        [JsonProperty] public bool UseDocker { get; private set; }
        [JsonProperty] public string DockerPath { get; private set; }
        [JsonProperty] public bool AutoUpdateDockerImage { get; private set; }
        [JsonProperty] public string SaveLocation { get; private set; }
        [JsonProperty] public string SourceLocation { get; private set; }
        [JsonProperty] public string MySqlServer { get; private set; }
        [JsonProperty] public int MySqlPort { get; private set; }
        [JsonProperty] public string MySqlPassword { get; private set; }

        public string MySqlConnection =>
            $"server={MySqlServer};user=modulr;database=Modulr;port={MySqlPort};password={MySqlPassword}";

        [JsonProperty] public string GoogleClientKey { get; private set; }
        [JsonProperty] public string GoogleSecret { get; private set; }
        // ReSharper disable twice InconsistentNaming
        [JsonProperty] public string reCAPCHASiteKey { get; private set; }
        [JsonProperty] public string reCAPCHASecretKey { get; private set; }
        [JsonProperty] public string HostedDomain { get; private set; }
        [JsonProperty] public int TimeoutAttempts { get; private set; }
        [JsonIgnore] public JObject RawData { get; }
        
        [JsonIgnore] public bool IsWindows { get; private set; }

        [JsonIgnore] private readonly ILogger<ModulrConfig> _logger;

        public ModulrConfig(ILogger<ModulrConfig> logger, string file = "config.json", bool verify = true)
        {
            var json = File.ReadAllText(file);
            JsonConvert.PopulateObject(json, this);
            RawData = JObject.Parse(json);
            File.WriteAllText(file, JsonConvert.SerializeObject(this, Formatting.Indented));
            _logger = logger;
            if (verify)
                VerifyConfig();
        }

        private readonly string[] _dockerWinPath = {
            @"C:\Program Files\Docker\Docker\resources\docker.exe",
            @"C:\Program Files\Docker\docker.exe",
            @"C:\Program Files\Docker\Docker\resources\bin\com.docker.cli.exe"
        };
        
        private readonly string[] _dockerLinPath = {
            @"/usr/bin/docker", 
            @"/usr/local/bin/docker"
        };
        
        private readonly string[] _javaWinPath = {
            @"C:\Program Files\Docker\Docker\resources\docker.exe",
            @"C:\Program Files\Docker\docker.exe",
            @"C:\Program Files\Docker\Docker\resources\bin\com.docker.cli.exe"
        };
        
        private readonly string[] _javaLinPath = {
            @"/usr/lib/jvm/java-11-openjdk-amd64", 
            @"/usr/local/bin/docker"
        };

        private void VerifyConfig()
        {
            CheckDocker();
        }

        private void CheckDocker()
        {
            IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            if (UseDocker && (DockerPath == null || !File.Exists(DockerPath)))
            {
                _logger.LogWarning($"Docker executable not set or found from config! Current value: {DockerPath}\n" +
                                   "Searching for a valid executable...");
                var dockerFound = false;
                
                var dockerInstalls = IsWindows ? _dockerWinPath : _dockerLinPath;
                foreach (var path in dockerInstalls)
                    if (File.Exists(path))
                    {
                        DockerPath = path;
                        dockerFound = true;
                        _logger.LogInformation($"Found Docker install at {DockerPath}");
                        break;
                    }
                
                if (!dockerFound)
                {
                    _logger.LogWarning("Docker executable not found! Falling back to local jails.");
                    UseDocker = false;
                }
            }
        }

        private void CheckJava()
        {
            IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            if (!UseDocker && (JDKPath == null || !Directory.Exists(JDKPath)))
            {
                _logger.LogWarning($"JDK home not set or found from config! Current value: {JDKPath}\n" +
                                   "Searching for a valid Java home...");
                var dockerFound = false;
                
                var dockerInstalls = IsWindows ? _dockerWinPath : _dockerLinPath;
                foreach (var path in dockerInstalls)
                    if (File.Exists(path))
                    {
                        DockerPath = path;
                        dockerFound = true;
                        _logger.LogInformation($"Found Java install at {JDKPath}");
                        break;
                    }
                
                if (!dockerFound)
                {
                    _logger.LogCritical("Java home not found! Modulr will probably not function properly.");
                    UseDocker = false;
                }
            }
        }
    }
}