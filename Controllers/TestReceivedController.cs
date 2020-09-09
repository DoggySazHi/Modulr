using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modulr.Models;
using Modulr.Tester;

namespace Modulr.Controllers
{
    [ApiController]
    [Route("/Tester")]
    public class TestReceivedController : ControllerBase
    {
        private readonly ILogger<TestReceivedController> _logger;

        public TestReceivedController(ILogger<TestReceivedController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Get()
        {
            return "Ohayo!";
        }
        
        [HttpPost("Upload")]
        public async Task<string> Post([FromForm] TesterFiles input)
        {
            if (input == null || !input.IsLikelyValid())
                return ">:[ not nice";
            var path = JavaUtils.GetDummyFolder();
            for (var i = 0; i < input.Files.Count; i++)
            {
                var file = input.Files[i];
                if (file.Length > 8 * 1024 * 1024) continue;
                var fileName = input.FileNames[i];
                var outputPath = Path.Join(path, fileName);
                await using var stream = new FileStream(outputPath, FileMode.Create);
                await file.CopyToAsync(stream);
            }
            return $">:] nice. found {input.Files.Count} files";
        }
    }
}