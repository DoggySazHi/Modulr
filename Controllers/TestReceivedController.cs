using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modulr.Models;
using Modulr.Tester;
using Newtonsoft.Json;

namespace Modulr.Controllers
{
    [ApiController]
    [Route("/Tester")]
    public class TestReceivedController : ControllerBase
    {
        private readonly ILogger<TestReceivedController> _logger;
        private readonly JavaUtils _java;
        private readonly MySqlTestQuery _query;

        public TestReceivedController(ILogger<TestReceivedController> logger, JavaUtils java, MySqlTestQuery query)
        {
            _logger = logger;
            _java = java;
            _query = query;
        }

        [HttpGet("GetTest")]
        public async Task<string> Get(int id)
        {
            var test = await _query.GetTest(id);
            if(test != null)
                return JsonConvert.SerializeObject(test);
            Response.StatusCode = 404;
            return "{}";
        }
        
        [HttpPost("Upload")]
        public async Task<string> Post([FromForm] TesterFiles input)
        {
            if (input == null || !input.IsLikelyValid())
                return Fail(400, ">:[ not nice");

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
                System.IO.File.Copy($"TestingSource/{file}", Path.Join(srcPath, file));
                input.FileNames.Add(file);
            }

            var output = _java.DockerTest(path, input.FileNames.ToArray());
            return output;
        }

        private string Fail(int response, string message)
        {
            Response.StatusCode = response;
            return message;
        }
    }
}