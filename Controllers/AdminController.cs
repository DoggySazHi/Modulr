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
            
            AddFolderToRouter("error", "StaticViews/views/error");
            AddFolderToRouter("", "RestrictedViews/views");
            AddFolderToRouter("img", "RestrictedViews/img");
            AddFolderToRouter("js", "RestrictedViews/js");
            AddFolderToRouter("css", "RestrictedViews/css");
        }

        [HttpGet]
        public override async Task<IActionResult> Get(string page)
        {
            if(Router.Count == 0)
                SetupRouter();
            
            if (!await this.IsAdmin(_query))
            {
                Response.StatusCode = 403;
                return await base.Get("error/403.html");
            }

            return await base.Get(page);
        }
    }
}