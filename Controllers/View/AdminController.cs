using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modulr.Controllers.Auth;
using Modulr.Tester;

namespace Modulr.Controllers.View;

[ApiController]
[Route("/Admin/{**page}")]
public class AdminController : HTMLController
{
    private readonly SqlQuery _query;
    private readonly PasswordManager _manager;
        
    public AdminController(ILogger<HTMLController> logger, SqlQuery query, ModulrConfig config, PasswordManager manager) : base(logger, config)
    {
        _query = query;
        _manager = manager;
    }
        
    protected override void SetupRouter()
    {
        Router.Clear();
            
        Router.Add("", "RestrictedViews/views/admin.html");
        Router.Add("home", "RestrictedViews/views/admin.html");
        Router.Add("system", "RestrictedViews/views/settings.html");
            
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
            
        if (!await this.VerifySession(_manager) || !await this.VerifyAdmin(_query))
        {
            Response.StatusCode = 403;
            return await base.Get("error/403.html");
        }

        return await base.Get(page);
    }
}