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
        private readonly Captcha _captcha;

        public UserController(SqlQuery query, PasswordManager manager, Captcha captcha)
        {
            _query = query;
            _manager = manager;
            _captcha = captcha;
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
        
        [HttpGet("GetCurrentUser")]
        public async Task<User> GetCurrentUser()
        {
            var result = await _query.ResolveUser(this.GetLoginCookie());
            return result;
        }

        [HttpPost("LogOut")]
        public async Task Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (await this.VerifySession(_manager))
            {
                await _query.LogoutUser(this.GetModulrID());
                return;
            }
            Response.StatusCode = 403;
        }
        
        [HttpPost("Login")]
        public async Task Login(LoginEvent login)
        {
            if (!await login.IsLikelyValid(_captcha))
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
        
        [HttpPost("Register")]
        public async Task<string> Register(RegisterUser register)
        {
            if (!await register.IsLikelyValid(_captcha))
            {
                Response.StatusCode = 400;
                return "Invalid response!";
            }

            var modulrID = await _query.UserExists(register.Email);

            if (modulrID != 0)
            {
                Response.StatusCode = 403;
                return "Account is already registered!";
            }

            modulrID = await _query.Register(register.Name, register.Email);
            await _manager.SetPassword(modulrID, register.Password);
            return "";
        }
    }
}