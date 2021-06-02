using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Modulr.Tester
{
    public class LocalJail : ModulrJail
    {
        private const string StartCompile = "{0} ========== BEGIN COMPILING {1} ==========";
        private const string FailCompile = "!! ======= FAILED COMPILATION!!! ======= !!";
        private const string EndCompile = "{0} ========== END COMPILING {1} ==========";
        private const string StartTest = "{0} ========== BEGIN TEST ==========";
        private const string FailTest = "{0} ========== FAILED COMPILATION - NO TEST ==========";
        private const string EndTest = "{0} ========== END TEST ==========";

        private const string JavaCUnix = "-cp *:. -Xlint:all -Xmaxwarns 100";
        private const string JavaExecUnix = "-Xmx64M -cp *:. com.williamle.modulr.stipulator.Startup --log-level INFO --use-to-string TRUE --allow-rw FALSE --real-time";

        private const string JavaCWin = "-cp *;. -Xlint:all -Xmaxwarns 100";
        private const string JavaExecWin = "-Xmx64M -cp *;. com.williamle.modulr.stipulator.Startup --log-level INFO --use-to-string TRUE --allow-rw FALSE --real-time";

        private readonly Task _task;
        private Process _process;
        private readonly string _sourceFolder;
        private readonly string[] _files;
        private string _randomKey;
        
        public LocalJail() {}

        public LocalJail(string sourceFolder, string connectionID = null, params string[] files) : base(sourceFolder, connectionID, files)
        {
            _sourceFolder = sourceFolder;
            _files = files;
            _task = Task.Run(async () => await Main());
        }

        private async Task Main()
        {
            GenerateKey();

            var src = Path.Join(_sourceFolder, "source");
            foreach (var file in Directory.EnumerateFiles(src))
                File.Move(file, Path.Combine(_sourceFolder, Path.GetFileName(file)));

            var stipulator = Path.Combine(_sourceFolder, "Modulr.Stipulator.jar");
            File.Copy("Docker/Modulr.Stipulator.jar", stipulator);
            
            var compile = true;
            foreach (var file in _files)
            {
                if (!compile)
                    break;
                if (file.EndsWith(".java"))
                    compile &= await Compile(file);
            }
            
            foreach (var file in Directory.EnumerateFiles(_sourceFolder))
                if (file.EndsWith(".java"))
                    File.Delete(file);

            if (!compile)
            {
                SendUpdate(string.Format(FailTest, _randomKey));
                return;
            }

            await Run();
            
            File.Delete(stipulator);
        }

        private void GenerateKey()
        {
            var random = new Random();
            const string chars = "ABCDEF0123456789";
            var stringChars = new char[6];

            for (var i = 0; i < stringChars.Length; i++)
                stringChars[i] = chars[random.Next(chars.Length)];

            _randomKey = new string(stringChars);
        }

        private async Task Run()
        {
            SendUpdate(string.Format(StartTest, _randomKey));
            await Execute(Path.Combine(Path.Join(Config.JDKPath, "bin"), "java"),
                $"{(Config.IsWindows ? JavaExecWin : JavaExecUnix)}");
            SendUpdate(string.Format(EndTest, _randomKey));
        }

        private async Task<bool> Compile(string file)
        {
            SendUpdate(string.Format(StartCompile, _randomKey, file));
            var pass = await Execute(Path.Combine(Path.Join(Config.JDKPath, "bin"), "javac"), $"{(Config.IsWindows ? JavaCWin : JavaCUnix)} \"{file}\"");
            if (!pass)
                SendUpdate(FailCompile);
            SendUpdate(string.Format(EndCompile, _randomKey, file));
            return pass;
        }
        
        private async Task<bool> Execute(string exec, string args)
        {
            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = exec,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = Path.GetFullPath(_sourceFolder)
                }
            };

            _process.OutputDataReceived += (_, info) =>
            {
                SendUpdate(info.Data);
            };
            _process.ErrorDataReceived += (_, info) =>
            {
                SendUpdate(info.Data);
            };

            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
            
            await _process.WaitForExitAsync();
            return _process.ExitCode == 0;
        }

        public override void Wait()
        {
            _task.Wait();
        }

        public override void Dispose()
        {
            _task?.Dispose();
            _process?.Dispose();
            GC.SuppressFinalize(this);
        }

        private protected override string InternalInit()
        {
            if (Config.AutoUpdateDockerImage)
                DownloadModulrStipulator();
            if (File.Exists("Docker/Modulr.Stipulator.jar"))
                return "Modulr.Stipulator jar available.";
            return
                "Modulr.Stipulator not available. Please enable AutoUpdateDockerImage or place Modulr.Stipulator.jar in the Docker folder.";
        }
    }
}