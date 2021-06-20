using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modulr.Models;
using Modulr.Tester;

namespace Modulr.Controllers
{
    [ApiController]
    [Authorize]
    [Route("/Admin/Tester")]
    public class AdminTestController : ControllerBase
    {
        private readonly SqlQuery _query;
        private readonly ModulrConfig _config;
        private readonly GoogleAuth _auth;
        
        public AdminTestController(SqlQuery query, ModulrConfig config, GoogleAuth auth)
        {
            _query = query;
            _config = config;
            _auth = auth;
        }
        
        [HttpPost("Add")]
        public async Task<int> OnAdd([FromBody] UpdateTesterFiles input)
        {
            if(!await this.IsAdmin(_query))
            {
                Response.StatusCode = 403;
                return -1;
            }

            if (!input.IsLikelyValid())
            {
                Response.StatusCode = 400;
                return -1;
            }
            
            return await _query.AddTest(input.TestName, input.TestDescription, input.Included, input.Testers, input.Required);
        }

        [HttpPost("GetAll")]
        public async Task<List<AdminStipulatable>> GetTests([FromBody] TestQuery login)
        {
            if(!await this.IsAdmin(_query))
            {
                Response.StatusCode = 403;
                return null;
            }
            
            if ((await _auth.Verify(login.AuthToken)).Status != GoogleAuth.LoginStatus.Success)
            {
                Response.StatusCode = 403;
                return null;
            }
            
            var tests = await _query.GetAllTests();
            var validatedTests = new List<AdminStipulatable>();
            foreach (var test in tests)
            {
                var adminStipulatable = new AdminStipulatable(test);
                adminStipulatable.Validate(_config);
                validatedTests.Add(adminStipulatable);
            }
            
            return validatedTests;
        }
        
        [HttpPost("Get")]
        public async Task<AdminStipulatable> GetTest([FromBody] TestQuery login)
        {
            if(!await this.IsAdmin(_query))
            {
                Response.StatusCode = 403;
                return null;
            }
            
            if ((await _auth.Verify(login.AuthToken)).Status != GoogleAuth.LoginStatus.Success)
            {
                Response.StatusCode = 403;
                return null;
            }
            
            var test = await _query.GetTest(login.TestID);
            
            if (test == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            
            var adminStipulatable = new AdminStipulatable(test);
            adminStipulatable.Validate(_config);
            return adminStipulatable;
        }
        
        [HttpPut("Update")]
        public async Task<bool> UpdateTest([FromBody] UpdateTesterFiles input)
        {
            if(!await this.IsAdmin(_query))
            {
                Response.StatusCode = 403;
                return false;
            }

            if (!input.IsLikelyValid())
            {
                Response.StatusCode = 400;
                return false;
            }

            return await _query.UpdateTest(input.TestID, input.TestName, input.TestDescription, input.Included, input.Testers, input.Required);
        }

        [HttpDelete("Delete")]
        public async Task<bool> DeleteTest([FromBody] int id)
        {
            if(!await this.IsAdmin(_query))
            {
                Response.StatusCode = 403;
                return false;
            }

            return await _query.DeleteTest(id);
        }

        [HttpPost("UploadSource")]
        [RequestSizeLimit(33554432)] // 32 MiB max
        public async Task<string> FileUploadSource([FromForm] TesterFiles input)
            => await FileUpload(input, _config.SourceLocation, true);

        [HttpPost("UploadInclude")]
        [RequestSizeLimit(33554432)] // 32 MiB max
        public async Task<string> FileUploadInclude([FromForm] TesterFiles input)
            => await FileUpload(input, _config.IncludeLocation);

        private async Task<string> FileUpload(TesterFiles input, string baseLocation, bool verify = false)
        {
            if (input.IsEmpty())
                return "WARNING: No files were found, nothing was uploaded!";
            if (!input.IsLikelyValid())
                return Fail(400, ">:[ not nice");
            
            if(!await this.IsAdmin(_query))
            {
                Response.StatusCode = 403;
                return null;
            }

            var auth = await _auth.Verify(input.AuthToken);
            if (auth.Status != GoogleAuth.LoginStatus.Success)
                return Fail(403, "Login needed!");

            var test = await _query.GetTest(input.TestID);
            var output = new StringBuilder();

            for (var i = 0; i < input.Files.Count; i++)
            {
                var file = input.Files[i];

                var fileName = input.FileNames[i] ?? Path.GetFileName(file.FileName);
                var sourcePath = Path.Join(baseLocation, "" + input.TestID);
                Directory.CreateDirectory(sourcePath);
                var outputPath = Path.Combine(sourcePath, fileName);
                var backupPath = outputPath;
                if (System.IO.File.Exists(backupPath))
                {
                    var counter = 0;
                    do
                    {
                        counter++;
                        backupPath = Path.Combine(sourcePath, $"old-{counter}-{fileName}");
                    } while (System.IO.File.Exists(backupPath));
                    System.IO.File.Move(outputPath, backupPath);
                    output.AppendLine($"WARNING: {fileName} existed already, made a copy at {backupPath}.");
                }

                if (verify && test != null && test.RequiredFiles.Contains(fileName))
                    output.AppendLine(
                        $"WARNING: {fileName} is a required file; it will not be copied during stipulation!");
                await using var stream = new FileStream(outputPath, FileMode.Create);
                await file.CopyToAsync(stream);
                output.AppendLine($"Successfully uploaded {fileName} to {outputPath}!");
            }

            return output.ToString();
        }
        
        private string Fail(int response, string message)
        {
            Response.StatusCode = response;
            return message;
        }
    }
}