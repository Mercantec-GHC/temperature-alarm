using Api.DBAccess;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : Controller
    {
        private readonly DbAccess _dbAccess;

        public HealthController(DbAccess dbAccess)
        {
            _dbAccess = dbAccess;
        }

        [HttpGet("API")]
        public async Task<IActionResult> HealthAPI() { return Ok(true); }

        [HttpGet("DB")]
        public async Task<IActionResult> HealthDB() { return Ok(_dbAccess.Test()); }
    }
}
