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
    public class DockerJail : ModulrJail
    {
        private readonly BlockingCollection<string> _logQueue;
        private readonly Process _process;
        
        private static bool _selfInit;
        private static bool _isEnterprise;
        
        public DockerJail(string sourceFolder, string connectionID = null, params string[] files) : base(sourceFolder, connectionID, files)
        {
            if (!_selfInit)
                Initialize();

            _logQueue = new BlockingCollection<string>();
            
            var args = $"run --rm -v \"{Path.Join(Path.GetFullPath(sourceFolder), "\\source").ToLower().Replace("\\", "" + Path.DirectorySeparatorChar)}:/src/files\" modulrjail {string.Join(' ', files)}";
            if (_isEnterprise)
                args =
                    $"run --rm -v {Path.Join(Path.GetFullPath(sourceFolder), "\\source")}:c:\\src\\files modulrjail {string.Join(' ', files)}";
            
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
            if (ConnectionID == null) return;
            try
            {
                WebSocket.SendUpdate(ConnectionID, data).ContinueWithoutAwait(_ => ConnectionID = null);
            }
            catch (Exception)
            {
                ConnectionID = null;
            }
        }

        [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public override string Initialize()
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

        public override string GetAllOutput() => GetAllOutput(_logQueue);

        public override void Wait() => _process.WaitForExit();

        public override void Dispose()
        {
            _logQueue?.Dispose();
            _process?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}