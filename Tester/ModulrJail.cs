using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Modulr.Tester
{
    public class ModulrJail : IDisposable
    {
        private readonly BlockingCollection<string> _logQueue = new BlockingCollection<string>();
        private readonly Process _process;
        
        public static ModulrConfig Config { private get; set; }
        private static bool SelfInit;
        
        public ModulrJail(string sourceFolder, params string[] files)
        {
            if (!SelfInit)
                Initialize();
            SelfInit = true;
            
            var args = $"run --rm -v \"{Path.Join(Path.GetFullPath(sourceFolder), "/source").ToLower()}:/src/files\" modulrjail {string.Join(' ', files)}";
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

            _process.OutputDataReceived += (sender, info) => _logQueue.Add(info.Data);
            _process.ErrorDataReceived += (sender, info) => _logQueue.Add(info.Data);

            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
        }

        private static void Initialize()
        {
            if (!Directory.Exists("Docker"))
                return;
            
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Config.DockerPath,
                    Arguments = "build -t modulrjail .",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = "Docker"
                }
            };
            
            process.Start();
            process.WaitForExit();
        }

        public string GetAllOutput()
        {
            var output = new StringBuilder();
            _logQueue.CompleteAdding();
            foreach (var line in _logQueue.GetConsumingEnumerable())
            {
                if (line == null)
                    continue;
                output.Append(line);
                output.Append('\n');
            }
            
            return output.ToString();
        }

        public void Wait() => _process.WaitForExit();

        public void Dispose()
        {
            _logQueue?.Dispose();
            _process?.Dispose();
        }
    }
}