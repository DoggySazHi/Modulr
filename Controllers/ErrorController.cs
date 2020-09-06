using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Modulr.Controllers
{
    [ApiController]
    [Route("/Error/{id:int}")]
    public class ErrorController : ControllerBase
    {
        private readonly ILogger<HTMLController> _logger;
        private readonly Dictionary<int, string> _router = new Dictionary<int, string>();

        public ErrorController(ILogger<HTMLController> logger)
        {
            _logger = logger;
            
            // HTML Routes
            _router.Add(404, "StaticViews/Errors/error404.html");
            _router.Add(500, "StaticViews/Errors/error500.html");
        }

        [HttpGet]
        public ContentResult Get(int code)
        {
            try
            {
                var found = _router.TryGetValue(code, out var file);
                if (!found)
                    throw new FileNotFoundException();
                using var reader = new StreamReader(file);
                return base.Content(reader.ReadToEnd(), "text/html");
            }
            catch (FileNotFoundException)
            {
                var file = _router[500];
                using var reader = new StreamReader(file);
                return base.Content(reader.ReadToEnd(), "text/html");
            }
        }
    }
}