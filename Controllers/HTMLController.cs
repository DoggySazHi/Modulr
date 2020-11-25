using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Modulr.Controllers
{
    public abstract class HTMLController : ControllerBase
    {
        private readonly ILogger<HTMLController> _logger;
        protected readonly Dictionary<string, string> Router = new Dictionary<string, string>();

        protected HTMLController(ILogger<HTMLController> logger)
        {
            _logger = logger;
        }

        protected abstract void SetupRouter();

        protected void AddFolderToRouter(string root, string folder)
        {
            var files = Directory.EnumerateFiles(folder);
            foreach (var file in files)
            {
                var key = (string.IsNullOrWhiteSpace(root) ? "" : root + '/') + Path.GetFileName(file);
                key = key.ToLower();
                var value = $"{file}".Replace('\\', '/');
                var success = Router.TryAdd(key, value);
                if (!success)
                    _logger.LogWarning($"Router found a duplicate route for {key}:\n- Old: {Router[key]}\n- New: {value}");
            }
            var directories = Directory.EnumerateDirectories(folder);
            foreach (var directory in directories)
                AddFolderToRouter((root == "" ? "" : root + '/') + Path.GetDirectoryName(directory)?.Replace('\\', '/'), $"{directory}");
        }

        [HttpGet]
        public virtual async Task<ContentResult> Get(string page)
        {
            if(Router.Count == 0)
                SetupRouter();
            
            page ??= "";
            page = page.ToLower();
            try
            {
                var found = Router.TryGetValue(page, out var file);
                if (!found)
                    throw new FileNotFoundException();
                return base.Content(await Templater(file), GetMIME(file));
            }
            catch (FileNotFoundException)
            {
                var text = (await Get("staticviews/views/404.html")).Content;
                Response.StatusCode = 404;
                return base.Content(text, "text/html");
            }
        }

        private static string GetMIME(string file)
        {
            var provider = new FileExtensionContentTypeProvider();
            return !provider.TryGetContentType(file, out var contentType) ? "application/octet-stream" : contentType;
        }
        
        private enum TemplateCode { Fail, Include, Title }

        private async Task<string> Templater(string file)
        {
            var text = await System.IO.File.ReadAllTextAsync(file);
            if (!file.EndsWith("html"))
                return text;
            
            string title = null;
            
            var evaluator = new MatchEvaluator(match =>
            {
                var (code, output) = TemplateInterpreter(match);
                switch (code)
                {
                    case TemplateCode.Include:
                        return output;
                    case TemplateCode.Title:
                        title = output;
                        return "";
                    default:
                        return "";
                }
            });
            
            var newPage = Regex.Replace(text, "{({.*})}", evaluator,
                RegexOptions.IgnorePatternWhitespace,
                Regex.InfiniteMatchTimeout);
            var html = new HtmlDocument();
            html.LoadHtml(newPage);
            var titleNode = html.DocumentNode.SelectSingleNode("//head/title");
            if (title != null && titleNode != null)
                titleNode.InnerHtml = title;
            return html.DocumentNode.OuterHtml;
        }

        private (TemplateCode Code, string Output) TemplateInterpreter(Match match)
        {
            var json = match.Result("$1");
            try
            {
                var obj = JObject.Parse(json);
                var file = obj["include"]?.ToString();
                if (file != null)
                    //return Get(file).GetAwaiter().GetResult().Content;
                    return (TemplateCode.Include, System.IO.File.ReadAllText(file));
                var title = obj["title"]?.ToString();
                if (title != null)
                    return (TemplateCode.Title, title);
                throw new JsonException("Could not understand command!");
            }
            catch (JsonException e)
            {
                _logger.Log(LogLevel.Error, "Failed to parse JSON!", e);
                return (TemplateCode.Fail, "");
            }
        }
    }
}