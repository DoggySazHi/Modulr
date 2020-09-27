using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modulr.Models;
using Modulr.Tester;

namespace Modulr.Controllers
{
    [ApiController]
    [Route("/Google")]
    public class GoogleLoginController : ControllerBase
    {
        private readonly ILogger<GoogleLoginController> _logger;
        private readonly ModulrConfig _config;
        private readonly GoogleAuth _auth;

        public GoogleLoginController(ILogger<GoogleLoginController> logger, ModulrConfig config, GoogleAuth auth)
        {
            _logger = logger;
            _config = config;
            _auth = auth;
        }

        [HttpGet("GetKey")]
        public string GetClientKey() => $"{{\"client_id\": \"{_config.GoogleClientKey}\"}}";

        [HttpPost("Login")]
        public async Task<LoginMessage> Login([FromBody] string token)
        {
            var result = await _auth.Verify(token);
            return result.Status switch
            {
                GoogleAuth.LoginStatus.BadAudience => LoginResult(403, "Invalid audience!"),
                GoogleAuth.LoginStatus.BadIssuer => LoginResult(403, "Invalid issuer!"),
                GoogleAuth.LoginStatus.ExpiredToken => LoginResult(403, "Token is expired!"),
                GoogleAuth.LoginStatus.BadDomain => LoginResult(403, "Invalid GSuite domain!"),
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