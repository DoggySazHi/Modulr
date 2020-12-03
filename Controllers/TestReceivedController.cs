using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Modulr.Models;
using Modulr.Tester;

namespace Modulr.Controllers
{
    [ApiController]
    [Route("/Tester")]
    public class TestReceivedController : ControllerBase
    {
        private readonly JavaUtils _java;
        private readonly MySqlQuery _query;
        private readonly GoogleAuth _auth;
        private readonly ModulrConfig _config;

        public TestReceivedController(JavaUtils java, MySqlQuery query, GoogleAuth auth, ModulrConfig config)
        {
            _java = java;
            _query = query;
            _auth = auth;
            _config = config;
        }

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
        
        [HttpPost("Upload")]
        public async Task<string> FileUpload([FromForm] TesterFiles input)
        {
            if (input == null || !input.IsLikelyValid())
                return Fail(400, ">:[ not nice");

            var (status, user) = await _auth.Verify(input.AuthToken);
            if (status != GoogleAuth.LoginStatus.Success)
                return Fail(403, "Login needed!");

            var attempts = await _query.GetTimeOut(user.Subject);
            if (_config.TimeoutAttempts >= 1 && attempts.TestsRemaining <= 0)
                return Fail(403, "You are on a cooldown!");

            var test = await _query.GetTest(input.TestID);
            if (test == null)
                return Fail(404, "Failed to find Test ID!");
            
            var path = _java.GetDummyFolder();
            var srcPath = Path.Join(path, "source");
            Directory.CreateDirectory(srcPath);
            for (var i = 0; i < input.Files.Count; i++)
            {
                var file = input.Files[i];
                if (file.Length > 8 * 1024 * 1024) continue;
                var fileName = input.FileNames[i];
                var outputPath = Path.Join(srcPath, fileName);
                await using var stream = new FileStream(outputPath, FileMode.Create);
                await file.CopyToAsync(stream);
            }
            
            foreach (var file in test.TesterFiles)
            {
                System.IO.File.Copy(Path.Join(_config.SourceLocation, file), Path.Join(srcPath, file));
                input.FileNames.Add(file);
            }

            var output = _java.DockerTest(path, test.TesterFiles.ToArray());
            await _query.DecrementAttempts(user.Subject);
            return output;
        }

        private string Fail(int response, string message)
        {
            Response.StatusCode = response;
            return message;
        }
    }
}