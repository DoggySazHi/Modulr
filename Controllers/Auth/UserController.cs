using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Modulr.Models;

namespace Modulr.Controllers.Auth
{
    [ApiController]
    [Route("/Users")]
    public class UserController : ControllerBase
    {
        private readonly SqlQuery _query;
        private readonly GoogleAuth _auth;

        public UserController(SqlQuery query, GoogleAuth auth)
        {
            _query = query;
            _auth = auth;
        }

        [HttpPost("GetTimeout")]
        public async Task<UserTimeout> GetTimeout([FromBody] string token)
        {
            var (status, user) = await _auth.Verify(token);
            if (status == GoogleAuth.LoginStatus.Success)
                return await _query.GetTimeOut(user.Subject);
            Response.StatusCode = 403;
            return null;
        }

        [HttpPost("LogOut")]
        public async Task Logout([FromBody] string token)
        {
            var (status, user) = await _auth.Verify(token);
            if (status == GoogleAuth.LoginStatus.Success)
            {
                var modulrUser = await _query.ResolveUser(user.Subject);
                await HttpContext.SignOutAsync();
                await _query.LogoutUser(modulrUser.ID);
            }
            Response.StatusCode = 403;
        }
    }
}