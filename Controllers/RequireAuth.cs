using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modulr.Models;

namespace Modulr.Controllers
{
    [ApiController]
    [Authorize]
    [Route("admin")]
    public class RequireAuth : ControllerBase
    {
        private readonly MySqlQuery _query;
        
        public RequireAuth(MySqlQuery query)
        {
            _query = query;
        }
        
        [HttpGet]
        public async Task<ContentResult> AdminPage()
        {
            if (!await IsAdmin())
            {
                var error = await System.IO.File.ReadAllTextAsync("StaticViews/views/error/404.html");
                Response.StatusCode = 403;
                return base.Content(error, "text/html");
            }
            var page = await System.IO.File.ReadAllTextAsync("StaticViews/restrictedviews/admin.html");
            return base.Content(page, "text/html");
        }

        private async Task<bool> IsAdmin()
            => await _query.GetRole(GetIdentity()) == Role.Admin;

        private string GetIdentity()
            => User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}