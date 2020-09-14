using System;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth;
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

        public GoogleLoginController(ILogger<GoogleLoginController> logger, ModulrConfig config)
        {
            _logger = logger;
            _config = config;
        }

        [HttpGet("GetKey")]
        public string GetClientKey() => $"{{\"client_id\": \"{_config.GoogleClientKey}\"}}";

        [HttpPost("Callback")]
        public string GoogleCallback(string message)
        {
            _logger.LogInformation(message);
            return "Mukyu!";
        }

        [HttpPost("Login")]
        public async Task<LoginMessage> Login([FromBody] string token)
        {
            if (token == null)
                return LoginResult(400, "Invalid data!");
            
            var validation = await GoogleJsonWebSignature.ValidateAsync(token);

            if (validation == null)
                return LoginResult(403, "Could not verify login with Google!");
            if (!validation.AudienceAsList.Contains(_config.GoogleClientKey))
                return LoginResult(403, "Audience of key is invalid!");
            if (validation.Issuer != "accounts.google.com" && validation.Issuer != "https://accounts.google.com")
                return LoginResult(403, "Invalid issuer!");
            if (validation.ExpirationTimeSeconds == null || validation.ExpirationTimeSeconds < DateTimeOffset.Now.ToUnixTimeSeconds())
                return LoginResult(403, "Token is expired!");
            if (!string.IsNullOrEmpty(_config.HostedDomain) && validation.HostedDomain != _config.HostedDomain)
                return LoginResult(403, "Account is not of target GSuite domain!");
            
            return LoginResult(200);
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