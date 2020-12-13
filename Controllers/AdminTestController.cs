using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        private readonly MySqlQuery _query;
        private readonly ModulrConfig _config;
        private readonly GoogleAuth _auth;
        
        public AdminTestController(MySqlQuery query, ModulrConfig config, GoogleAuth auth)
        {
            _query = query;
            _config = config;
            _auth = auth;
        }
        
        [HttpPost("Add")]
        public async Task<int> OnUpload([FromForm] SourceTesterFiles input)
        {
            if(!await this.IsAdmin(_query))
            {
                Response.StatusCode = 403;
                return -1;
            }
            
            var testerFiles = new List<string>();
            
            foreach(var file in input.Extra)
                testerFiles.Add(await DownloadTester(file));
            foreach(var file in input.Testers)
                testerFiles.Add(await DownloadTester(file));

            return await _query.AddTest(input.TestName, testerFiles, input.Required);
        }

        private async Task<string> DownloadTester(IFormFile file)
        {
            if(!await this.IsAdmin(_query))
            {
                Response.StatusCode = 403;
                return null;
            }
            
            if (file.Length > 8 * 1024 * 1024) return null;
            var fileName = Path.GetFileName(file.FileName);
            var outputPath = Path.Join(_config.SourceLocation, fileName);
            for(var i = 2; System.IO.File.Exists(outputPath); i++)
                outputPath = Path.Join(_config.SourceLocation, $"{fileName}_{i}");
            await using var stream = new FileStream(outputPath, FileMode.Create);
            await file.CopyToAsync(stream);
            return Path.GetFileName(outputPath);
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

            return await _query.UpdateTest(input.TestID, input.TestName, input.Testers, input.Required);
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
        
        [HttpPost("Upload")]
        public async Task<string> FileUpload([FromForm] TesterFiles input)
        {
            if (input.IsEmpty())
                return "WARNING: No files were found, nothing was uploaded!";
            if (!input.IsLikelyValid())
                return Fail(400, ">:[ not nice");

            var auth = await _auth.Verify(input.AuthToken);
            if (auth.Status != GoogleAuth.LoginStatus.Success)
                return Fail(403, "Login needed!");

            var output = new StringBuilder();

            for (var i = 0; i < input.Files.Count; i++)
            {
                var file = input.Files[i];
                if (file.Length > 8 * 1024 * 1024) continue;
                var fileName = input.FileNames[i];
                var outputPath = Path.Join(_config.SourceLocation, fileName);
                var backupPath = outputPath;
                if (System.IO.File.Exists(backupPath))
                {
                    var counter = 0;
                    do
                    {
                        counter++;
                        backupPath = Path.Join(_config.SourceLocation, $"old-{counter}-{fileName}");
                    } while (System.IO.File.Exists(backupPath));
                    System.IO.File.Move(outputPath, backupPath);
                    output.AppendLine($"WARNING: {fileName} existed already, made a copy at {backupPath}.");
                }
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