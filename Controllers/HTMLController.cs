using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;

namespace Modulr.Controllers
{
    public abstract class HTMLController : ControllerBase
    {
        private readonly ILogger<HTMLController> _logger;
        protected readonly Dictionary<string, string> Router = new Dictionary<string, string>();

        protected HTMLController(ILogger<HTMLController> logger)
        {
            _logger = logger;
            if (Router.Count == 0)
                SetupRouter();
        }

        protected abstract void SetupRouter();

        protected void AddFolderToRouter(string root, string folder)
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
        public virtual async Task<ContentResult> Get(string page)
        {
            page ??= "";
            page = page.ToLower();
            try
            {
                var found = Router.TryGetValue(page, out var file);
                if (!found)
                    throw new FileNotFoundException(); 
                var text = await System.IO.File.ReadAllTextAsync(file);
                return base.Content(text, GetMIME(file));
            }
            catch (FileNotFoundException)
            {
                var text = await System.IO.File.ReadAllTextAsync("StaticViews/views/error/404.html");
                Response.StatusCode = 404;
                return base.Content(text, "text/html");
            }
        }

        private static string GetMIME(string file)
        {
            var provider = new FileExtensionContentTypeProvider();
            return !provider.TryGetContentType(file, out var contentType) ? "application/octet-stream" : contentType;
        }
    }
}