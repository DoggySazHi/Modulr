using System.Collections.Generic;
using System.IO;
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
    [Route("/Admin/AddTest")]
    public class AdminTestController : ControllerBase
    {
        private readonly MySqlQuery _query;
        private readonly ModulrConfig _config;
        
        public AdminTestController(MySqlQuery query, ModulrConfig config)
        {
            _query = query;
            _config = config;
        }
        
        [HttpPost("AddTest")]
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

        [HttpPost("GetTests")]
        public async Task<List<AdminStipulatable>> GetTests()
        {
            if(!await this.IsAdmin(_query))
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
        
        [HttpPut("UpdateTest")]
        public async Task<bool> UpdateTest(UpdateTesterFiles input)
        {
            if(!await this.IsAdmin(_query))
            {
                Response.StatusCode = 403;
                return false;
            }

            return await _query.UpdateTest(input.TestID, input.TestName, input.Testers, input.Required);
        }

        [HttpDelete("DeleteTest")]
        public async Task<bool> DeleteTest([FromBody] int id)
        {
            if(!await this.IsAdmin(_query))
            {
                Response.StatusCode = 403;
                return false;
            }

            return await _query.DeleteTest(id);
        }
    }
}