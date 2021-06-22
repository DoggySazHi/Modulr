using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Modulr.Models;
using Modulr.Tester;

namespace Modulr.Controllers.Auth
{
    [ApiController]
    [Route("/Google")]
    public class GoogleController : ControllerBase
    {
        private readonly ModulrConfig _config;
        private readonly GoogleAuth _auth;

        public GoogleController(ModulrConfig config, GoogleAuth auth)
        {
            _config = config;
            _auth = auth;
        }

        [HttpGet("GetKey")]
        public string GetClientKey() => $"{{\"client_id\": \"{_config.GoogleClientKey}\"}}";
        
        [HttpGet("GetCapchaKey")]
        public string GetCAPCHAKey() => _config.reCAPCHASiteKey;

        [HttpPost("Login")]
        public async Task<LoginMessage> Login([FromBody] string token)
        {
            var result = await _auth.Verify(token);
            return result.Status switch
            {
                GoogleAuth.LoginStatus.BadAudience => LoginResult(403, "Invalid audience!"),
                GoogleAuth.LoginStatus.BadDomain => LoginResult(403, "Invalid GSuite domain!"),
                GoogleAuth.LoginStatus.BadIssuer => LoginResult(403, "Invalid issuer!"),
                GoogleAuth.LoginStatus.Banned => LoginResult(403, "Account has been banned!"),
                GoogleAuth.LoginStatus.ExpiredToken => LoginResult(403, "Token is expired!"),
                GoogleAuth.LoginStatus.Invalid => LoginResult(400, "Invalid token!"),
                GoogleAuth.LoginStatus.Success => LoginResult(200),
                _ => LoginResult(400, "Invalid data!")
            };
        }

        private LoginMessage LoginResult(int status, string error = null)
        {
            Response.StatusCode = status;
            return new LoginMessage
            {
                Success = status < 400,
                Error = error
            };
        }
    }
}