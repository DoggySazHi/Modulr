using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;

namespace Modulr.Controllers
{
    [ApiController]
    
    [Route("/{**page}")]
    public class HTMLController : ControllerBase
    {
        private readonly ILogger<HTMLController> _logger;
        private readonly Dictionary<string, string> _router = new Dictionary<string, string>();

        public HTMLController(ILogger<HTMLController> logger)
        {
            // TODO Generate a router by iterating through files...
            _logger = logger;
            
            // HTML Routes
            _router.Add("", "StaticViews/index.html");
            _router.Add("index.html", "StaticViews/index.html");
            _router.Add("home", "StaticViews/index.html");
            _router.Add("testdemo", "StaticViews/tester.html");
            
            // Image Routes
            _router.Add("img/modulr.svg", "StaticViews/img/modulr.svg");
            _router.Add("modulr.svg", "StaticViews/img/modulr.svg");
            
            // Holy crap icon stuff
            _router.Add("favicon.ico", "StaticViews/img/favicon.ico");
            _router.Add("android-chrome-192x192.png", "StaticViews/img/android-chrome-192x192.png");
            _router.Add("android-chrome-512x512.png", "StaticViews/img/android-chrome-512x512.png");
            _router.Add("apple-touch-icon.png", "StaticViews/img/apple-touch-icon.png");
            _router.Add("favicon-16x16.png", "StaticViews/img/favicon-16x16.png");
            _router.Add("favicon-32x32.png", "StaticViews/img/favicon-32x32.png");
            _router.Add("site.webmanifest", "StaticViews/img/site.webmanifest");

            // JS Routes
            _router.Add("js/main.js", "StaticViews/js/main.js");
            _router.Add("js/tester.js", "StaticViews/js/tester.js");
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
                return base.Content(reader.ReadToEnd(), GetMIME(file));
            }
            catch (FileNotFoundException)
            {
                using var reader = new StreamReader("StaticViews/Errors/error404.html");
                Response.StatusCode = 404;
                return base.Content(reader.ReadToEnd(), "text/html");
            }
        }

        private string GetMIME(string file)
        {
            var provider = new FileExtensionContentTypeProvider();
            return !provider.TryGetContentType(file, out var contentType) ? "application/octet-stream" : contentType;
        }
    }
}