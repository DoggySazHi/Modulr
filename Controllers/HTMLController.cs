﻿using System;
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
        private static readonly Dictionary<string, string> _router = new Dictionary<string, string>();

        public HTMLController(ILogger<HTMLController> logger)
        {
            _logger = logger;
            if (_router.Count == 0)
                SetupRouter();
        }
        
        private void SetupRouter()
        {
            // So why not serve static? Because I need a way to serve my own routes.
            // Even though ASP.NET Core can do that too. Oh well.
            
            // HTML Routes
            _router.Add("", "StaticViews/views/index.html");
            _router.Add("home", "StaticViews/views/index.html");
            _router.Add("testdemo", "StaticViews/views/tester.html");

            // Holy crap icon stuff
            _router.Add("favicon.ico", "StaticViews/img/favicon.ico");
            _router.Add("android-chrome-192x192.png", "StaticViews/img/android-chrome-192x192.png");
            _router.Add("android-chrome-512x512.png", "StaticViews/img/android-chrome-512x512.png");
            _router.Add("apple-touch-icon.png", "StaticViews/img/apple-touch-icon.png");
            _router.Add("favicon-16x16.png", "StaticViews/img/favicon-16x16.png");
            _router.Add("favicon-32x32.png", "StaticViews/img/favicon-32x32.png");
            _router.Add("site.webmanifest", "StaticViews/img/site.webmanifest");
            
            // All other files
            AddFolderToRouter("", "StaticViews/views");
            AddFolderToRouter("img", "StaticViews/img");
            AddFolderToRouter("js", "StaticViews/js");
            AddFolderToRouter("css", "StaticViews/css");
        }

        private void AddFolderToRouter(string root, string folder)
        {
            var files = Directory.EnumerateFiles(folder);
            foreach (var file in files)
            {
                var key = (string.IsNullOrWhiteSpace(root) ? "" : root + '/') + Path.GetFileName(file);
                var value = $"{file}".Replace('\\', '/');
                var success = _router.TryAdd(key, value);
                if (!success)
                    _logger.LogWarning($"Router found a duplicate route for {key}:\n- Old: {_router[key]}\n- New: {value}");
            }
            var directories = Directory.EnumerateDirectories(folder);
            foreach (var directory in directories)
                AddFolderToRouter(root + '/' + Path.GetDirectoryName(directory), $"{directory}");
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

        private static string GetMIME(string file)
        {
            var provider = new FileExtensionContentTypeProvider();
            return !provider.TryGetContentType(file, out var contentType) ? "application/octet-stream" : contentType;
        }
    }
}