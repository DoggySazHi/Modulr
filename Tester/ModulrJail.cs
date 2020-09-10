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
        private Process _process;
        
        public static TesterConfiguration Config { private get; set; }
        
        public ModulrJail(string sourceFolder, params string[] files)
        {
            var args = $"run --rm -v \"{Path.Join(Path.GetFullPath(sourceFolder), "/source").ToLower()}:/src/files\" modulrjail {string.Join(' ', files)}";
            Console.WriteLine(args);
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

            _process.OutputDataReceived += (sender, args) => _logQueue.Add(args.Data);
            _process.ErrorDataReceived += (sender, args) => _logQueue.Add(args.Data);

            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
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