using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Modulr.Models;
using Modulr.Tester;

namespace Modulr.Controllers
{
    [ApiController]
    [Authorize]
    [Route("/Admin/System")]
    public class AdminSystemController : ControllerBase
    {
        private readonly MySqlQuery _query;
        private readonly GoogleAuth _auth;
        private readonly IHostApplicationLifetime _app;
        
        public AdminSystemController(MySqlQuery query, ModulrConfig config, GoogleAuth auth, IHostApplicationLifetime app)
        {
            _query = query;
            ModulrJail.Config = config;
            _auth = auth;
            _app = app;
        }

        [HttpPost("RebuildContainer")]
        public async Task<string> RebuildContainer([FromBody] BasicAuth login)
        {
            if(!await this.IsAdmin(_query))
            {
                Response.StatusCode = 403;
                return null;
            }
            
            if ((await _auth.Verify(login.AuthToken)).Status != GoogleAuth.LoginStatus.Success)
            {
                Response.StatusCode = 403;
                return null;
            }

            return ModulrJail.Initialize();
        }
        
        [HttpPost("Shutdown")]
        public async Task Shutdown([FromBody] BasicAuth login)
        {
            if(!await this.IsAdmin(_query))
            {
                Response.StatusCode = 403;
                return;
            }

            if ((await _auth.Verify(login.AuthToken)).Status != GoogleAuth.LoginStatus.Success)
            {
                Response.StatusCode = 403;
                return;
            }

            _ = Task.Run(async () =>
            {
                await Task.Delay(5000);
                _app.StopApplication();
            });
        }
    }
}