using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modulr.Controllers.Auth;
using Modulr.Tester;

namespace Modulr.Controllers.View;

[ApiController]
[Route("/Tester/{**page}")]
public class TesterController : HTMLController
{
    private readonly PasswordManager _manager;
        
    public TesterController(ILogger<TesterController> logger, ModulrConfig config, PasswordManager manager) : base(logger, config)
    {
        _manager = manager;
    }

    protected override void SetupRouter()
    {
        Router.Clear();
            
        // HTML Routes
        Router.Add("student-test", "StaticViews/views/student-test.html");
    }
        
    [HttpGet]
    public override async Task<IActionResult> Get(string page)
    {
        if(Router.Count == 0)
            SetupRouter();
            
        if (!await this.VerifySession(_manager))
        {
            Response.StatusCode = 403;
            return Redirect("/?error=Login%20is%20required%20to%20be%20on%20that%20page!");
        }

        return await base.Get(page);
    }
}