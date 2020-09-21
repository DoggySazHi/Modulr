﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Modulr.Models;

namespace Modulr.Controllers
{
    [ApiController]
    [Route("/Users")]
    public class UserController : ControllerBase
    {
        private readonly MySqlQuery _query;
        private readonly GoogleAuth _auth;

        public UserController(MySqlQuery query, GoogleAuth auth)
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
    }
}