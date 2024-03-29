﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Modulr.Models;
using Modulr.Tester;

namespace Modulr.Controllers.Auth;

[ApiController]
[Route("/Google")]
public class GoogleController : ControllerBase
{
    private readonly ModulrConfig _config;
    private readonly GoogleAuth _auth;
    private readonly PasswordManager _manager;

    public GoogleController(ModulrConfig config, GoogleAuth auth, PasswordManager manager)
    {
        _config = config;
        _auth = auth;
        _manager = manager;
    }

    [HttpGet("GetKey")]
    public string GetClientKey() => $"{{\"client_id\": \"{_config.GoogleClientKey}\"}}";
        
    [HttpGet("GetCaptchaKey")]
    public string GetCAPTCHAKey() => _config.reCAPTCHASiteKey;

    [HttpPost("Login")]
    public async Task<LoginMessage> Login([FromBody] string token)
    {
        var (status, user) = await _auth.Verify(token);
        switch (status)
        {
            case GoogleAuth.LoginStatus.BadAudience:
                return LoginResult(403, "Invalid audience!");
            case GoogleAuth.LoginStatus.BadDomain:
                return LoginResult(403, "Invalid GSuite domain!");
            case GoogleAuth.LoginStatus.BadIssuer:
                return LoginResult(403, "Invalid issuer!");
            case GoogleAuth.LoginStatus.Banned:
                return LoginResult(403, "Account has been banned!");
            case GoogleAuth.LoginStatus.ExpiredToken:
                return LoginResult(403, "Token is expired!");
            case GoogleAuth.LoginStatus.Invalid:
                return LoginResult(400, "Invalid token!");
            case GoogleAuth.LoginStatus.Success:
                await Login(user);
                return LoginResult(200);
            default:
                return LoginResult(400, "Invalid data!");
        }
    }

    private async Task Login(User user)
    {
        var cookie = await _manager.GenerateCookie(user.ID);
        await this.LoginUser(user, cookie);
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