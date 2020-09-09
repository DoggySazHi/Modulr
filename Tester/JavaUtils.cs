using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Modulr.Tester
{
    public class TesterConfiguration
    {
        public string JDKPath { get; set; }
        public bool UseDocker { get; set; }
        public string DockerPath { get; set; }
        public string SaveLocation { get; set; }
    }
    
    public static class JavaUtils
    {
        private static readonly TesterConfiguration Config;
        private static readonly Random Rng;
        
        static JavaUtils()
        {
            using var sr = new StreamReader("config.json");
            var json = sr.ReadToEnd();
            Config = JsonSerializer.Deserialize<TesterConfiguration>(json);
            Rng = new Random();
            Clean();
        }

        private static void Clean()
        {
            if (Directory.Exists(Config.SaveLocation))
                Directory.Delete(Config.SaveLocation);
            Directory.CreateDirectory(Config.SaveLocation);
        }

        public static string DockerTest(string sourceFolder, params string[] files)
        {
            // We're going to make the assumption that sourceFolder is sanitized.
            // Mainly because it's created within Modulr.
            // Probably requires to be relative as well.
            
            var message = Process.Start(
                new ProcessStartInfo
                {
                    FileName = Config.DockerPath,
                    Arguments = $"run --rm -v src:/src/files modulrjail {string.Join(' ', files)}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WorkingDirectory = sourceFolder
                })?.StandardOutput.ReadToEnd();
            return message;
        }

        private const int TempDirLength = 6;
        
        public static string GetDummyFolder()
        {
            string path;
            var sb = new StringBuilder(TempDirLength);
            do
            {
                sb.Clear();
                for (var i = 0; i < TempDirLength; i++)
                    sb.Append((char) Rng.Next('0', 'Z'));
                path = Path.Join(Config.SaveLocation, sb.ToString());
            } while (Directory.Exists(path));
            Directory.CreateDirectory(path);
            return path;
        }

        private static bool IsWindows()
            => Environment.OSVersion.Platform == PlatformID.Win32NT;
    }
}