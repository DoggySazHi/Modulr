using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modulr.Tester;

namespace Modulr.Controllers
{
    [ApiController]
    [Authorize]
    [Route("/Admin/{**page}")]
    public class AdminController : HTMLController
    {
        private readonly MySqlQuery _query;
        
        public AdminController(ILogger<HTMLController> logger, MySqlQuery query) : base(logger)
        {
            _query = query;
        }
        
        protected override void SetupRouter()
        {
            Router.Clear();
            
            Router.Add("", "RestrictedViews/views/admin.html");
            Router.Add("home", "RestrictedViews/views/admin.html");
            
            AddFolderToRouter("", "RestrictedViews/views");
            AddFolderToRouter("img", "RestrictedViews/img");
            AddFolderToRouter("js", "RestrictedViews/js");
            AddFolderToRouter("css", "RestrictedViews/css");
        }

        [HttpGet]
        public override async Task<ContentResult> Get(string page)
        {
            if (!await this.IsAdmin(_query))
            {
                var error = await System.IO.File.ReadAllTextAsync("StaticViews/views/error/403.html");
                Response.StatusCode = 403;
                return base.Content(error, "text/html");
            }

            return await base.Get(page);
        }
    }
}