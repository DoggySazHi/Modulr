using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Modulr.Models;

namespace Modulr.Controllers.Auth
{
    [ApiController]
    [Route("/Users")]
    public class UserController : ControllerBase
    {
        private readonly SqlQuery _query;
        private readonly PasswordManager _manager;

        public UserController(SqlQuery query, PasswordManager manager)
        {
            _query = query;
            _manager = manager;
        }

        [HttpPost("GetTimeout")]
        public async Task<UserTimeout> GetTimeout([FromBody] string token)
        {
            var user = _manager.VerifySession()
            if (status == GoogleAuth.LoginStatus.Success)
                return new UserTimeout(user.TestsTimeout, user.TestsRemaining);
            Response.StatusCode = 403;
            return null;
        }

        [HttpPost("LogOut")]
        public async Task Logout([FromBody] string token)
        {
            var (status, user) = await _auth.Verify(token);
            if (status == GoogleAuth.LoginStatus.Success)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                await _query.LogoutUser(user.ID);
            }
            Response.StatusCode = 403;
        }
        
        [HttpPost("Login")]
        public async Task Login([FromBody] string token)
        {
            var (status, user) = await _auth.Verify(token);
            if (status == GoogleAuth.LoginStatus.Success)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                await _query.LogoutUser(user.ID);
            }
            Response.StatusCode = 403;
        }
    }
}