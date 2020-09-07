using System.Text.Json;
using System.Text.Json.Serialization;

namespace Modulr.Tester
{
    internal class TesterConfiguration
    {
        private string JDKPath { get; set; }
        private bool UseDocker { get; set; }
    }
    
    public static class JavaUtils
    {
        private static readonly TesterConfiguration Config;
        
        static JavaUtils()
        {
            Config = JsonSerializer.Deserialize<TesterConfiguration>("config.json");
        }
    }
}