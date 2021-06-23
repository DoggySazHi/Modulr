using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Modulr.Controllers;
using Modulr.Controllers.Auth;
using Modulr.Models;

namespace Modulr.Tester
{
    public static class ControllerExtensions
    {
        public static async Task<bool> VerifySession(this ControllerBase controller, PasswordManager manager)
            => await manager.VerifySession(GetModulrID(controller), GetLoginCookie(controller));

        public static async Task<bool> VerifyAdmin(this ControllerBase controller, SqlQuery query)
            => await query.GetRole(GetModulrID(controller)) == Role.Admin;

        public static int GetModulrID(this ControllerBase controller)
        {
            var tryReadID = int.TryParse(controller.User.FindFirstValue("ModulrID"), out var modulrID);
            return !tryReadID ? 0 : modulrID;
        }

        public static string GetLoginCookie(this ControllerBase controller)
            => controller.User.FindFirstValue("Token");

        public static async Task LoginUser(this ControllerBase controller, User user, string cookie)
        {
            const string cookieScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            
            var identity = new ClaimsIdentity(new List<Claim>
            {
                new("ModulrID", "" + user.ID),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Role, $"{(int) user.Role}"),
                new("Token", cookie)
            }, cookieScheme);
            
            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                IsPersistent = true,
                IssuedUtc = DateTimeOffset.UtcNow,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14),
                RedirectUri = ""
            };
            
            await controller.HttpContext.SignInAsync(
                cookieScheme, 
                new ClaimsPrincipal(identity), 
                authProperties);
        }
    }
}