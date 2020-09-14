using System.IO;
using Newtonsoft.Json;
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Modulr.Tester
{
    public class ModulrConfig
    {
        [JsonProperty]
        public string JDKPath { get; private set; }
        [JsonProperty]
        public bool UseDocker { get; private set; }
        [JsonProperty]
        public string DockerPath { get; private set; }
        [JsonProperty]
        public string SaveLocation { get; private set; }
        [JsonProperty]
        public string MySqlServer { get; private set; }
        [JsonProperty]
        public int MySqlPort { get; private set; }
        [JsonProperty]
        public string MySqlPassword { get; private set; }
        public string MySqlConnection => $"server={MySqlServer};user=modulr;database=Modulr;port={MySqlPort};password={MySqlPassword}";
        [JsonProperty]
        public string GoogleClientKey { get; private set; }
        [JsonProperty]
        public string GoogleSecret { get; private set; }
        [JsonProperty]
        public string HostedDomain { get; private set; }

        public static ModulrConfig Build(string file = "config.json")
        {
            var json = File.ReadAllText(file);
            return JsonConvert.DeserializeObject<ModulrConfig>(json);
        }
    }
}