using System;
using System.IO;

namespace Modulr.Tester
{
    public class JavaUtils
    {
        private readonly ModulrConfig _config;
        private readonly Random _rng;
        
        public JavaUtils(ModulrConfig config)
        {
            ModulrJail.Config = config;
            _config = config;
            _rng = new Random();
            Clean();
        }

        private void Clean()
        {
            if (Directory.Exists(_config.SaveLocation))
                Directory.Delete(_config.SaveLocation, true);
            Directory.CreateDirectory(_config.SaveLocation);
        }

        public string DockerTest(string sourceFolder, string connectionID = null, params string[] files)
        {
            // We're going to make the assumption that sourceFolder is sanitized.
            // Mainly because it's created within Modulr.
            // Probably requires to be relative as well.
            
            var jail = new ModulrJail(sourceFolder, connectionID, files);
            jail.Wait();
            var output = jail.GetAllOutput();
            jail.Dispose();
            return output;
        }

        private const int TempDirLength = 6;
        
        public string GetDummyFolder()
        {
            string path;
            do
            {
                path = Path.Join(_config.SaveLocation, GetRandomString());
            } while (Directory.Exists(path));
            Directory.CreateDirectory(path);
            return path;
        }

        private const string RNGChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private string GetRandomString()
        {
            var output = new char[TempDirLength];

            for (var i = 0; i < output.Length; i++)
                output[i] = RNGChars[_rng.Next(RNGChars.Length)];

            return new string(output);
        }
    }
}