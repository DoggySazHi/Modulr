using System;
using System.IO;
using System.Net;
using Modulr.Hubs.Workers;

namespace Modulr.Tester
{
    /// <summary>
    /// Represents a generic Jail, for running/testing code.
    /// </summary>
    public abstract class ModulrJail : IDisposable
    {
        private static readonly string MODULR_STIPULATOR_GITHUB = "https://github.com/DoggySazHi/Modulr.Stipulator/releases/latest/download/Modulr.Stipulator.jar";

        public static ModulrConfig Config { protected get; set; }
        public static TestWorker WebSocket { protected get; set; }
        
        protected string ConnectionID;

        protected ModulrJail()
        {
            // Only exists to initialize.
        }
        
        protected ModulrJail(string sourceFolder, string connectionID = null, params string[] files)
        {
            ConnectionID = connectionID;
        }

        public static ModulrJail Build(string sourceFolder, string connectionID = null, params string[] files)
        {
            if (Config.UseDocker)
                return new DockerJail(sourceFolder, connectionID, files);
            return new LocalJail(sourceFolder, connectionID, files);
        }

        /**
         * Override this method to initialize.
         */
        public virtual string Initialize()
        {
            return "This jail does not override the static initialization method.";
        }

        public abstract string GetAllOutput();
        
        /// <summary>
        /// Block execution until the jail has finished running.
        /// </summary>
        public abstract void Wait();
        
        public abstract void Dispose();

        /// <summary>
        /// Download the jar file which emulates JUnit.
        /// </summary>
        protected static void DownloadModulrStipulator()
        {
            if (!Config.AutoUpdateDockerImage)
                return;
            
            using var client = new WebClient();
            client.DownloadFile(MODULR_STIPULATOR_GITHUB, "Docker/Modulr.Stipulator.jar");
        }

        /// <summary>
        /// Convert a file from CRLF line endings to LF.
        /// Mainly useful for servers which run on Linux, or just using the Linux Docker instance.
        /// </summary>
        /// <param name="file"></param>
        protected static void ToLF(string file)
        {
            using (var sr = new StreamReader(file))
            using (var sw = new StreamWriter($"{file}.lf"))
                sw.Write(sr.ReadToEnd().Replace("\r\n","\n"));
            File.Delete(file);
            File.Move($"{file}.lf", file);
        }
    }
}