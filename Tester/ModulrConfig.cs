using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Modulr.Tester
{
    public class ModulrConfig
    {
        [JsonProperty] public int Port { get; private set; }
        [JsonProperty] public string JDKPath { get; private set; }
        [JsonProperty] public bool UseDocker { get; private set; }
        [JsonProperty] public string DockerPath { get; private set; }
        [JsonProperty] public bool AutoUpdateDockerImage { get; private set; }
        [JsonProperty] public string SaveLocation { get; private set; }
        [JsonProperty] public string SourceLocation { get; private set; }
        [JsonProperty] public string IncludeLocation { get; private set; }
        [JsonProperty] public string SqlServer { get; private set; }
        [JsonProperty] public int SqlPort { get; private set; }
        [JsonProperty] public string SqlPassword { get; private set; }
        [JsonProperty] public bool UseMySql { get; private set; }

        [JsonIgnore]
        public string MySqlConnection
        {
            get
            {
                var output = new MySqlConnectionStringBuilder {
                    Server = SqlServer,
                    UserID = "modulr",
                    Port = Convert.ToUInt32(SqlPort),
                    Password = SqlPassword,
                    Database = "Modulr"
                };
                return output.ConnectionString;
            }
        }

        [JsonIgnore]
        public string SqlConnection
        {
            get
            {
                var output = new SqlConnectionStringBuilder
                {
                    DataSource = $"{SqlServer},{SqlPort}",
                    IntegratedSecurity = true,
                    UserID = "Modulr",
                    Password = SqlPassword,
                    InitialCatalog = "Modulr"
                };
                return output.ConnectionString;
            }
        }

        [JsonProperty] public string GoogleClientKey { get; private set; }
        [JsonProperty] public string GoogleSecret { get; private set; }
        // ReSharper disable twice InconsistentNaming
        [JsonProperty] public string reCAPTCHASiteKey { get; private set; }
        [JsonProperty] public string reCAPTCHASecretKey { get; private set; }
        [JsonProperty] public string HostedDomain { get; private set; }
        [JsonProperty] public int TimeoutAttempts { get; private set; }
        [JsonProperty] public string[] WebSocketDomains { get; private set; }
        [JsonIgnore] public JObject RawData { get; }
        
        [JsonIgnore] public bool IsWindows { get; private set; }

        [JsonIgnore] private readonly ILogger<ModulrConfig> _logger;

        public ModulrConfig(ILogger<ModulrConfig> logger, string file = "config.json", bool verify = true)
        {
            var json = File.ReadAllText(file);
            SetDefaultConfig();
            JsonConvert.PopulateObject(json, this);
            RawData = JObject.Parse(json);
            File.WriteAllText(file, JsonConvert.SerializeObject(this, Formatting.Indented));
            _logger = logger;
            if (verify)
                VerifyConfig();
        }

        /// <summary>
        /// Only applies to items that should have a value.
        /// </summary>
        private void SetDefaultConfig()
        {
            Port = 5001;
            SqlPort = 1433;
            SqlServer = "localhost";
            UseMySql = true;
            TimeoutAttempts = -1;

            SourceLocation = "TestingSource";
            SaveLocation = "TestingInput";
            IncludeLocation = "TestingInclude";
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

        private void VerifyConfig()
        {
            CheckDocker();
            CheckJava();
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

                var javaHomes = new List<(string version, string dir)>();

                if (!IsWindows)
                {
                    var directories = Directory.GetDirectories("/usr/lib/jvm");
                    foreach (var home in directories)
                    {
                        var version = Regex.Match(home, @"(\d+)").Value;
                        var bin = Path.Join(home, "bin");
                        if (File.Exists(Path.Combine(bin, "java")) && File.Exists(Path.Combine(bin, "javac")))
                            javaHomes.Add((version, home));
                    }
                }
                else
                {
                    var directories = Directory.GetDirectories(@"C:\Program Files\Java");
                    foreach (var home in directories)
                    {
                        var versionString = "";
                        var subVersionString = "";

                        foreach (Match match in Regex.Matches(home, @"([\d\.]+)"))
                        {
                            if (match.Value == string.Empty)
                                continue;
                            if (match.Value.Contains("."))
                                versionString = match.Value;
                            else
                                subVersionString = match.Value;
                        }
                        
                        var bin = Path.Join(home, "bin");
                        if (File.Exists(Path.Combine(bin, "java")) && File.Exists(Path.Combine(bin, "javac")))
                            javaHomes.Add((versionString + "." + subVersionString, home));
                    }
                }

                if (javaHomes.Count > 0)
                {
                    JDKPath = javaHomes.Aggregate(
                            (a, b) => CompareSemanticVersion(a.version, b.version) > 0 ? a : b)
                        .dir;
                    _logger.LogInformation($"Found JDK home at {JDKPath}");
                }
                else
                {
                    _logger.LogCritical("Java home not found! Modulr will probably not function properly.");
                }
            }
        }

        private static int CompareSemanticVersion(string a, string b)
        {
            var verA = a.Split('.');
            var verB = b.Split('.');

            for (var i = 0; i < Math.Min(verA.Length, verB.Length); ++i)
            {
                var numATry = int.TryParse(verA[i], out var numA);
                var numBTry = int.TryParse(verA[i], out var numB);
                if (!numATry)
                    numA = 0;
                if (!numBTry)
                    numB = 0;
                if (numA < numB)
                    return -1;
                if (numA > numB)
                    return 1;
            }

            return 0;
        }
    }
}