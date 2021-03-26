using System.Collections.Generic;
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
                await Task.Delay(1000);
                _app.StopApplication();
            });
        }
        
        [HttpPost("GetAllUsers")]
        public async Task<IEnumerable<User>> GetAllUsers([FromBody] BasicAuth login)
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

            return await _query.GetAllUsers();
        }
        
        [HttpPost("UpdateUser")]
        public async Task UpdateUser([FromBody] UpdateUser user)
        {
            if(!await this.IsAdmin(_query))
            {
                Response.StatusCode = 403;
                return;
            }

            if ((await _auth.Verify(user.AuthToken)).Status != GoogleAuth.LoginStatus.Success)
            {
                Response.StatusCode = 403;
                return;
            }

            await _query.UpdateUser(user);
        }
        
        [HttpPost("ResetUserTimeout")]
        public async Task ResetUserTimeout([FromBody] UpdateUser user)
        {
            if(!await this.IsAdmin(_query))
            {
                Response.StatusCode = 403;
                return;
            }

            if ((await _auth.Verify(user.AuthToken)).Status != GoogleAuth.LoginStatus.Success)
            {
                Response.StatusCode = 403;
                return;
            }

            await _query.ResetTimeOut(user.ID);
        }
    }
}