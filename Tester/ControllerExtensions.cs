using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Modulr.Controllers;
using Modulr.Models;

namespace Modulr.Tester
{
    public static class ControllerExtensions
    {
        public static async Task<bool> IsAdmin(this ControllerBase controller, SqlQuery query)
            => await query.GetRole(GetIdentity(controller)) == Role.Admin;

        public static string GetIdentity(this ControllerBase controller)
            => controller.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}