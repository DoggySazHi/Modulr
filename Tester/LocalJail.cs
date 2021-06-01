using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Modulr.Tester
{
    public class LocalJail : ModulrJail
    {
        private readonly string START_COMPILE = " ========== BEGIN COMPILING $var ==========";
        private readonly string FAIL_COMPILE = "!! ======= FAILED COMPILATION!!! ======= !!";
        private readonly string END_COMPILE = " ========== END COMPILING $var ==========";
        private readonly string START_TEST = " ========== BEGIN TEST ==========";
        private readonly string FAIL_TEST = " ========== FAILED COMPILATION - NO TEST ==========";
        private readonly string END_TEST = "d ========== END TEST ==========";

        private readonly string JAVA_C_UNIX = "-cp *:. -Xlint:all -Xmaxwarns 100";
        private readonly string JAVA_EXEC_UNIX =
            "-Xmx64M -cp *:. com.williamle.modulr.stipulator.Startup --log-level INFO --use-to-string TRUE --allow-rw FALSE --real-time";

        private readonly string JAVA_C_WIN = "-cp *;. -Xlint:all -Xmaxwarns 100";
        private readonly string JAVA_EXEC_WIN =
            "-Xmx64M -cp *;. com.williamle.modulr.stipulator.Startup --log-level INFO --use-to-string TRUE --allow-rw FALSE --real-time";

        private Task _task;
        private Process _process;
        private string _sourceFolder;
        private string _findFolder;
        private string _randomKey;

        public LocalJail(string sourceFolder, string connectionID = null, params string[] files) : base(sourceFolder, connectionID, files)
        {
            _sourceFolder = sourceFolder;
            _task = Task.Run(Main);
        }

        private void Main()
        {
            GenerateKey();
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
            SendUpdate(_randomKey + START_TEST);
            await Execute(Path.Combine(Path.Join(Config.JDKPath, "bin"), "java"),
                $"{(Config.IsWindows ? JAVA_EXEC_WIN : JAVA_EXEC_UNIX)}");
            SendUpdate(_randomKey + END_TEST);
        }

        private async Task<bool> Compile(string file)
        {
            SendUpdate(_randomKey + START_COMPILE);
            var pass = await Execute(Path.Combine(Path.Join(Config.JDKPath, "bin"), "javac"), $"{(Config.IsWindows ? JAVA_C_WIN : JAVA_C_UNIX)} {file}");
            if (!pass)
                SendUpdate(FAIL_COMPILE);
            SendUpdate(_randomKey + END_COMPILE);
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
                    WorkingDirectory = _sourceFolder
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
    }
}