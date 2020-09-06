using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modulr.Models;

namespace Modulr.Controllers
{
    [ApiController]
    [Route("/testing")]
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
        
        [HttpPost]
        public string Post([FromBody] Test testResults)
        {
            _logger.Log(LogLevel.Information, testResults.ToString());
            return "Hello!";
        }
    }
}