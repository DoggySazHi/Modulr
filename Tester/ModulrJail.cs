using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Text;
using Modulr.Hubs.Workers;

namespace Modulr.Tester
{
    /// <summary>
    /// Represents a generic Jail, for running/testing code.
    /// </summary>
    public abstract class ModulrJail : IDisposable
    {
        private static readonly string MODULR_STIPULATOR_GITHUB = "https://github.com/DoggySazHi/Modulr.Stipulator/releases/latest/download/Modulr.Stipulator.jar";

        public static ModulrConfig Config { private protected get; set; }
        public static TestWorker WebSocket { private protected get; set; }
        
        private readonly BlockingCollection<string> _logQueue;
        private string _connectionID;

        private protected ModulrJail()
        {
            _logQueue = new BlockingCollection<string>();
        }
        
        private protected ModulrJail(string sourceFolder, string connectionID = null, params string[] files) : this()
        {
            _ = sourceFolder;
            _ = files;
            _connectionID = connectionID;
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
        private protected virtual string InternalInit()
        {
            return "This jail does not override the static initialization method.";
        }

        public static string Initialize()
        {
            return Config.UseDocker ? new DockerJail().InternalInit() : new LocalJail().InternalInit();
        }

        /// <summary>
        /// Block execution until the jail has finished running.
        /// </summary>
        public abstract void Wait();

        public virtual void Dispose()
        {
            _logQueue?.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Download the jar file which emulates JUnit.
        /// </summary>
        private protected static void DownloadModulrStipulator()
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
        /// <param name="file">The location where the file is located.</param>
        private protected static void ToLF(string file)
        {
            using (var sr = new StreamReader(file))
            using (var sw = new StreamWriter($"{file}.lf"))
                sw.Write(sr.ReadToEnd().Replace("\r\n","\n"));
            File.Delete(file);
            File.Move($"{file}.lf", file);
        }
        
        /// <summary>
        /// Send data using the WebSocket, if available.
        /// </summary>
        /// <param name="data">The new data to be pushed.</param>
        private protected void SendUpdate(string data)
        {
            if (_connectionID == null) return;
            _logQueue.Add(data);
            try
            {
                WebSocket.SendUpdate(_connectionID, data).ContinueWithoutAwait(_ => _connectionID = null);
            }
            catch (Exception)
            {
                _connectionID = null;
            }
        }
        
        private protected static string GetAllOutput(BlockingCollection<string> logQueue)
        {
            var output = new StringBuilder();
            logQueue.CompleteAdding();
            foreach (var line in logQueue.GetConsumingEnumerable())
            {
                if (line == null)
                    continue;
                output.Append(line);
                output.Append('\n');
            }
            
            return output.ToString();
        }

        public virtual string GetAllOutput() => GetAllOutput(_logQueue);
    }
}