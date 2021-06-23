using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Modulr.Controllers.Auth;
using Modulr.Controllers.View;
using Modulr.Models;
using Modulr.Tester;

namespace Modulr.Controllers
{
    [ApiController]
    [Route("/Tester")]
    public class TestReceivedController : ControllerBase
    {
        private readonly JavaUtils _java;
        private readonly SqlQuery _query;
        private readonly GoogleAuth _auth;
        private readonly ModulrConfig _config;
        private readonly Captcha _captcha;

        public TestReceivedController(JavaUtils java, SqlQuery query, GoogleAuth auth, ModulrConfig config, Captcha captcha)
        {
            _java = java;
            _query = query;
            _auth = auth;
            _config = config;
            _captcha = captcha;
        }

        /// <summary>
        /// Get information about a test.
        /// </summary>
        /// <param name="item">A POST request, consisting of an authentication token and test ID.</param>
        /// <returns>Data about the test requested.</returns>
        [HttpPost("GetTest")]
        public async Task<Stipulatable> GetTest([FromBody] TestQuery item)
        {
            if ((await _auth.Verify(item.AuthToken)).Status != GoogleAuth.LoginStatus.Success)
            {
                Response.StatusCode = 403;
                return null;
            }
            var test = await _query.GetTest(item.TestID);
            if(test != null)
                return test;
            Response.StatusCode = 404;
            return null;
        }
        
        /// <summary>
        /// Get all tests that are available for the user.
        /// </summary>
        /// <param name="login">A POST request, consisting of an authentication token.</param>
        /// <returns>A list of tests available.</returns>
        [HttpPost("GetAllTests")]
        public async Task<IEnumerable<Stipulatable>> GetAllTests([FromBody] TestQuery login)
        {
            if ((await _auth.Verify(login.AuthToken)).Status != GoogleAuth.LoginStatus.Success)
            {
                Response.StatusCode = 403;
                return null;
            }
            var test = await _query.GetAllTests();
            if(test != null)
                return test;
            Response.StatusCode = 404;
            return null;
        }
        
        /// <summary>
        /// Triggers when a user uploads their files.
        /// </summary>
        /// <param name="input">Data from a web form, consisting of the Test ID, files, websocket, and authentication token.</param>
        /// <returns>A string, representing the tester output.</returns>
        [HttpPost("Upload")]
        [RequestSizeLimit(8388608)] // 8 MiB
        public async Task<string> FileUpload([FromForm] TesterFiles input)
        {
            if (input == null || !input.IsLikelyValid())
                return Fail(400, ">:[ not nice");

            var (status, user) = await _auth.Verify(input.AuthToken);
            if (status != GoogleAuth.LoginStatus.Success)
                return Fail(403, "Login needed!");
            
            if (!await _captcha.VerifyCaptcha(input.CaptchaToken))
                return Fail(403, "reCAPTCHA token invalid!");

            if (_config.TimeoutAttempts >= 1 && user.TestsRemaining <= 0)
                return Fail(403, "You are on a cooldown!");

            var test = await _query.GetTest(input.TestID);
            if (test == null)
                return Fail(404, "Failed to find Test ID!");
            
            var path = _java.GetDummyFolder();
            var inputPath = Path.Join(path, "source");
            Directory.CreateDirectory(inputPath);
            for (var i = 0; i < input.Files.Count; i++)
            {
                var file = input.Files[i];
                if (file.Length > 8 * 1024 * 1024) continue;
                var fileName = input.FileNames[i];
                var outputPath = Path.Combine(inputPath, fileName);
                await using var stream = new FileStream(outputPath, FileMode.Create);
                await file.CopyToAsync(stream);
            }
            
            foreach (var file in test.TesterFiles)
            {
                var sourcePath = Path.Join(_config.SourceLocation, "" + input.TestID);
                var tester = Path.Combine(sourcePath, file);
                var testerOut = Path.Combine(inputPath, file);
                if (!System.IO.File.Exists(tester) || System.IO.File.Exists(testerOut))
                    continue;
                System.IO.File.Copy(tester, testerOut);
                input.FileNames.Add(file);
            }

            var output = _java.DockerTest(path, input.ConnectionID, test.TesterFiles.ToArray());
            await _query.DecrementAttempts(user.ID);
            return output;
        }
        
        /// <summary>
        /// Allow a user to download an included file.
        /// </summary>
        /// <param name="file">A file requested from the user.</param>
        /// <returns>Data representing the file, or an error message.</returns>
        [HttpPost("Download")]
        public async Task<IActionResult> FileDownload([FromBody] DownloadFile file)
        {
            if (file.File == null)
                return BadRequest("Bad Filename!!");

            var (status, _) = await _auth.Verify(file.AuthToken);
            if (status != GoogleAuth.LoginStatus.Success)
                return Forbid();

            var test = await _query.GetTest(file.TestID);
            if (test == null)
                return NotFound("Failed to find Test ID!");

            var fileName = Path.GetFileName(file.File);
            var path = Path.Combine(Path.Join(_config.IncludeLocation, "" + file.TestID), fileName!);
            if (!System.IO.File.Exists(path))
                return NotFound("File not found!");

            var stream = System.IO.File.OpenRead(path);
            return new FileStreamResult(stream, HTMLController.GetMIME(fileName));
        }

        /// <summary>
        /// A simple way to fail with a HTTP status.
        /// </summary>
        /// <param name="response">The HTTP status to return.</param>
        /// <param name="message">The content to send back to the user.</param>
        /// <returns>The string you passed as the message.</returns>
        private string Fail(int response, string message)
        {
            Response.StatusCode = response;
            return message;
        }
    }
}