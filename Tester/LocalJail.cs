using System.Diagnostics;

namespace Modulr.Tester
{
    public class LocalJail : ModulrJail
    {
        private readonly string START_COMPILE = "$randomkey ========== BEGIN COMPILING $var ==========";
        private readonly string FAIL_COMPILE = "!! ======= FAILED COMPILATION!!! ======= !!";
        private readonly string END_COMPILE = "$randomkey ========== END COMPILING $var ==========";
        private readonly string START_TEST = "$randomkey ========== BEGIN TEST ==========";
        private readonly string FAIL_TEST = "$randomkey ========== FAILED COMPILATION - NO TEST ==========";
        private readonly string END_TEST = "$randomkey ========== END TEST ==========";

        private readonly string JAVA_C_UNIX = "-cp *:. -Xlint:all -Xmaxwarns 100";
        private readonly string JAVA_EXEC_UNIX =
            "-Xmx64M -cp *:. com.williamle.modulr.stipulator.Startup --log-level INFO --use-to-string TRUE --allow-rw FALSE --real-time";

        private readonly string JAVA_C_WIN = "-cp *;. -Xlint:all -Xmaxwarns 100";
        private readonly string JAVA_EXEC_WIN =
            "-Xmx64M -cp *;. com.williamle.modulr.stipulator.Startup --log-level INFO --use-to-string TRUE --allow-rw FALSE --real-time";
        
        private Process _process;
        private string _sourceFolder;
        private string _findFolder;

        public LocalJail(string sourceFolder, string connectionID = null, params string[] files) : base(sourceFolder, connectionID, files)
        {
            
        }

        private void StartJava()
        {
            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Config.JDKPath,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = _sourceFolder
                }
            };

            _process.OutputDataReceived += (_, info) =>
            {
                LogQueue.Add(info.Data);
                SendUpdate(info.Data);
            };
            _process.ErrorDataReceived += (_, info) =>
            {
                LogQueue.Add(info.Data);
                SendUpdate(info.Data);
            };

            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
        }

        public override string GetAllOutput()
        {
            throw new System.NotImplementedException();
        }

        public override void Wait()
        {
            throw new System.NotImplementedException();
        }

        public override void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}