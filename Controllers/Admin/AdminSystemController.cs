using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Modulr.Controllers.Auth;
using Modulr.Models;
using Modulr.Tester;

namespace Modulr.Controllers.Admin
{
    [ApiController]
    [Authorize]
    [Route("/Admin/System")]
    public class AdminSystemController : ControllerBase
    {
        private readonly SqlQuery _query;
        private readonly PasswordManager _manager;
        private readonly IHostApplicationLifetime _app;
        
        public AdminSystemController(SqlQuery query, ModulrConfig config, PasswordManager manager, IHostApplicationLifetime app)
        {
            _query = query;
            ModulrJail.Config = config;
            _manager = manager;
            _app = app;
        }

        [HttpPost("RebuildContainer")]
        public async Task<string> RebuildContainer()
        {
            if (!await this.VerifySession(_manager) || !await this.VerifyAdmin(_query))
            {
                Response.StatusCode = 403;
                return null;
            }

            return ModulrJail.Initialize();
        }
        
        [HttpPost("Shutdown")]
        public async Task Shutdown()
        {
            if (!await this.VerifySession(_manager) || !await this.VerifyAdmin(_query))
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
        public async Task<IEnumerable<User>> GetAllUsers()
        {
            if (!await this.VerifySession(_manager) || !await this.VerifyAdmin(_query))
            {
                Response.StatusCode = 403;
                return null;
            }

            return await _query.GetAllUsers();
        }
        
        [HttpPost("UpdateUser")]
        public async Task UpdateUser([FromBody] UpdateUser user)
        {
            if (!await this.VerifySession(_manager) || !await this.VerifyAdmin(_query))
            {
                Response.StatusCode = 403;
                return;
            }

            await _query.UpdateUser(user);
        }
        
        [HttpPost("ResetUserTimeout")]
        public async Task ResetUserTimeout([FromBody] UpdateUser user)
        {
            if (!await this.VerifySession(_manager) || !await this.VerifyAdmin(_query))
            {
                Response.StatusCode = 403;
                return;
            }

            await _query.ResetTimeOut(user.ID);
        }
    }
}