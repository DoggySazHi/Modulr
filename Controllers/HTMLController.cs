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
        private static readonly Dictionary<string, string> Router = new Dictionary<string, string>();

        public HTMLController(ILogger<HTMLController> logger)
        {
            _logger = logger;
            if (Router.Count == 0)
                SetupRouter();
        }
        
        private void SetupRouter()
        {
            // So why not serve static? Because I need a way to serve my own routes.
            // Even though ASP.NET Core can do that too. Oh well.
            
            // HTML Routes
            Router.Add("", "StaticViews/views/index.html");
            Router.Add("home", "StaticViews/views/index.html");
            Router.Add("testdemo", "StaticViews/views/tester.html");
            Router.Add("student-test", "StaticViews/views/student-test.html");

            // Holy crap icon stuff
            AddFolderToRouter("", "StaticViews/img");

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
                var success = Router.TryAdd(key, value);
                if (!success)
                    _logger.LogWarning($"Router found a duplicate route for {key}:\n- Old: {Router[key]}\n- New: {value}");
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
                var found = Router.TryGetValue(page, out var file);
                if (!found)
                    throw new FileNotFoundException();
                using var reader = new StreamReader(file);
                return base.Content(reader.ReadToEnd(), GetMIME(file));
            }
            catch (FileNotFoundException)
            {
                using var reader = new StreamReader("StaticViews/views/error/404.html");
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