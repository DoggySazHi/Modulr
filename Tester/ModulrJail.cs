using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Text;
using Modulr.Hubs.Workers;

namespace Modulr.Tester
{
    public class ModulrJail : IDisposable
    {
        private readonly BlockingCollection<string> _logQueue;
        private readonly Process _process;
        private string _connectionID;
        
        public static ModulrConfig Config { private get; set; }
        public static TestWorker WebSocket { private get; set; }
        private static bool _selfInit;
        private static bool _isEnterprise;
        
        /**
         * This can be risky if auto-downloaded; we might add an option to disable auto-updates.
         */
        private static readonly string MODULR_STIPULATOR_GITHUB = "https://github.com/DoggySazHi/Modulr.Stipulator/releases/latest/download/Modulr.Stipulator.jar";

        public ModulrJail(string sourceFolder, string connectionID = null, params string[] files)
        {
            if (!_selfInit)
                Initialize();

            _logQueue = new BlockingCollection<string>();
            
            var args = $"run --rm -v \"{Path.Join(Path.GetFullPath(sourceFolder), "\\source").ToLower().Replace("\\", "" + Path.DirectorySeparatorChar)}:/src/files\" modulrjail {string.Join(' ', files)}";
            if (_isEnterprise)
                args =
                    $"run --rm -v {Path.Join(Path.GetFullPath(sourceFolder), "\\source")}:c:\\src\\files modulrjail {string.Join(' ', files)}";

            _connectionID = connectionID;
            
            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Config.DockerPath,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = sourceFolder
                }
            };

            _process.OutputDataReceived += (_, info) =>
            {
                _logQueue.Add(info.Data);
                SendUpdate(info.Data);
            };
            _process.ErrorDataReceived += (_, info) =>
            {
                _logQueue.Add(info.Data);
                SendUpdate(info.Data);
            };

            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
        }

        private void SendUpdate(string data)
        {
            if (_connectionID == null) return;
            try
            {
                WebSocket.SendUpdate(_connectionID, data).ContinueWithoutAwait(_ => _connectionID = null);
            }
            catch (Exception)
            {
                _connectionID = null;
            }
        }

        [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public static string Initialize()
        {
            var logQueue = new BlockingCollection<string>();

            var versionProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Config.DockerPath,
                    Arguments = "version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = "Docker"
                }
            };
            
            versionProcess.OutputDataReceived += (_, info) => logQueue.Add(info.Data);
            versionProcess.ErrorDataReceived += (_, info) => logQueue.Add(info.Data);
            
            versionProcess.Start();
            versionProcess.BeginOutputReadLine();
            versionProcess.BeginErrorReadLine();
            versionProcess.WaitForExit();

            var output = GetAllOutput(logQueue);
            logQueue.Dispose();
            logQueue = new BlockingCollection<string>();
            
            if (output.Contains("Enterprise"))
                _isEnterprise = true;

            if (!Directory.Exists("Docker")) {
                logQueue.Add("No Docker folder found; cannot rebuild!");
                return GetAllOutput(logQueue);
            }

            var dockerFile = _isEnterprise ? "DockerfileWS" : "Dockerfile";
            if (!_isEnterprise)
            {
                ToLF("Docker/CompileAndTest.sh");
                ToLF("Docker/Dockerfile");
            }
            
            DownloadModulrStipulator();

            var imageProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Config.DockerPath,
                    Arguments = $"build -f {dockerFile} -t modulrjail .",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = "Docker"
                }
            };
            
            imageProcess.OutputDataReceived += (_, info) => logQueue.Add(info.Data);
            imageProcess.ErrorDataReceived += (_, info) => logQueue.Add(info.Data);
            
            imageProcess.Start();
            imageProcess.BeginOutputReadLine();
            imageProcess.BeginErrorReadLine();
            imageProcess.WaitForExit();
            
            _selfInit = true;
            var output2 = GetAllOutput(logQueue);
            logQueue.Dispose();
            versionProcess.Dispose();
            imageProcess.Dispose();
            
            return output + "\n" + output2;
        }

        private static void DownloadModulrStipulator()
        {
            using var client = new WebClient();
            client.DownloadFile(MODULR_STIPULATOR_GITHUB, "Docker/Modulr.Stipulator.jar");
        }

        private static void ToLF(string file)
        {
            using (var sr = new StreamReader(file))
                using (var sw = new StreamWriter($"{file}.lf"))
                    sw.Write(sr.ReadToEnd().Replace("\r\n","\n"));
            File.Delete(file);
            File.Move($"{file}.lf", file);
        }

        private static string GetAllOutput(BlockingCollection<string> logQueue)
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

        public string GetAllOutput() => GetAllOutput(_logQueue);

        public void Wait() => _process.WaitForExit();

        public void Dispose()
        {
            _logQueue?.Dispose();
            _process?.Dispose();
        }
    }
}