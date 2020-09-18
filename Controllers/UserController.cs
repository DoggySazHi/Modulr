using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Modulr.Models;

namespace Modulr.Controllers
{
    [ApiController]
    [Route("/Users")]
    public class UserController : ControllerBase
    {
        private readonly MySqlQuery _query;

        public UserController(MySqlQuery query)
        {
            _query = query;
        }

        [HttpGet("GetTimeout")]
        public async Task<UserTimeout> GetTimeout(string token)
            => await _query.GetTimeOut(token);
    }
}