using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Modulr.Controllers
{
    [ApiController]
    [Route("/")]
    public class HTMLController : ControllerBase
    {
        private readonly ILogger<HTMLController> _logger;
        private readonly Dictionary<string, string> _router = new Dictionary<string, string>();

        public HTMLController(ILogger<HTMLController> logger)
        {
            _logger = logger;
            _router.Add("", "StaticViews/index.html");
        }

        [HttpGet]
        public ContentResult Get(string data = null)
        {
            try
            {
                using var reader = new StreamReader("StaticViews/index.html");
                return base.Content(reader.ReadToEnd(), "text/html");
            }
            catch (FileNotFoundException)
            {
                return base.Content("William didn't like that.");
            }
        }
        
        [HttpPost]
        public string Post()
        {
            return "Hello!";
        }
    }
}