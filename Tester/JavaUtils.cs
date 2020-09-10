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
        private static readonly Random RNG;
        
        static JavaUtils()
        {
            using var sr = new StreamReader("config.json");
            var json = sr.ReadToEnd();
            Config = JsonSerializer.Deserialize<TesterConfiguration>(json);
            ModulrJail.Config = Config;
            RNG = new Random();
            Clean();
        }

        private static void Clean()
        {
            if (Directory.Exists(Config.SaveLocation))
                Directory.Delete(Config.SaveLocation, true);
            Directory.CreateDirectory(Config.SaveLocation);
        }

        public static string DockerTest(string sourceFolder, params string[] files)
        {
            // We're going to make the assumption that sourceFolder is sanitized.
            // Mainly because it's created within Modulr.
            // Probably requires to be relative as well.
            
            var jail = new ModulrJail(sourceFolder, files);
            jail.Wait();
            var output = jail.GetAllOutput();
            jail.Dispose();
            return output;
        }

        private const int TempDirLength = 6;
        
        public static string GetDummyFolder()
        {
            string path;
            do
            {
                path = Path.Join(Config.SaveLocation, GetRandomString());
            } while (Directory.Exists(path));
            Directory.CreateDirectory(path);
            return path;
        }

        private const string RNGChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        private static string GetRandomString()
        {
            var output = new char[TempDirLength];

            for (var i = 0; i < output.Length; i++)
                output[i] = RNGChars[RNG.Next(RNGChars.Length)];

            return new string(output);
        }

        private static bool IsWindows()
            => Environment.OSVersion.Platform == PlatformID.Win32NT;
    }
}