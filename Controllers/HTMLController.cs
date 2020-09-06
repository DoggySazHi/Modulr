using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Modulr.Controllers
{
    [ApiController]
    [Route("/")]
    [Route("/{page}")]
    [Route("/css/{page}")]
    [Route("/img/{page}")]
    [Route("/js/{page}")]
    public class HTMLController : ControllerBase
    {
        private readonly ILogger<HTMLController> _logger;
        private readonly Dictionary<string, string> _router = new Dictionary<string, string>();

        public HTMLController(ILogger<HTMLController> logger)
        {
            _logger = logger;
            
            // HTML Routes
            _router.Add("", "StaticViews/index.html");
            _router.Add("index.html", "StaticViews/index.html");
            _router.Add("home", "StaticViews/index.html");
            
            // Image Routes
            _router.Add("modulr.svg", "StaticViews/img/modulr.svg");
            
            // JS Routes
            _router.Add("main.js", "StaticViews/js/main.js");
        }

        [HttpGet]
        public ContentResult Get(string page)
        {
            page ??= "";
            page = page.ToLower();
            try
            {
                var found = _router.TryGetValue(page, out var file);
                if (!found)
                    throw new FileNotFoundException();
                using var reader = new StreamReader(file);
                return base.Content(reader.ReadToEnd(), "text/html");
            }
            catch (FileNotFoundException)
            {
                using var reader = new StreamReader("StaticViews/Errors/error404.html");
                Response.StatusCode = 404;
                return base.Content(reader.ReadToEnd(), "text/html");
            }
        }
    }
}