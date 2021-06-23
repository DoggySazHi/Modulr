using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Modulr.Models;
using Modulr.Tester;

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
        public async Task<UserTimeout> GetTimeout()
        {
            if (await this.VerifySession(_manager))
            {
                var user = await _query.ResolveUser(this.GetLoginCookie());
                return new UserTimeout(user.TestsTimeout, user.TestsRemaining);
            }

            Response.StatusCode = 403;
            return null;
        }

        [HttpPost("LogOut")]
        public async Task Logout()
        {
            if (await this.VerifySession(_manager))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                await _query.LogoutUser(this.GetModulrID());
            }
            Response.StatusCode = 403;
        }
        
        [HttpPost("Login")]
        public async Task Login(LoginEvent login)
        {
            if (!login.IsLikelyValid())
            {
                Response.StatusCode = 400;
                return;
            }

            var modulrID = await _query.UserExists(login.Email);

            if (modulrID == 0)
            {
                Response.StatusCode = 403;
                return;
            }

            var loginCookie = await _manager.Login(modulrID, login.Password);
            
            if (loginCookie == null)
            {
                Response.StatusCode = 403;
                return;
            }

            var user = await _query.ResolveUser(loginCookie);
            await this.LoginUser(user, loginCookie);
        }
    }
}