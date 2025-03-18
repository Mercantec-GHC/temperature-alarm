using Microsoft.AspNetCore.Mvc;
using Models;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> GetDevices(int userId)
        {
            return Ok();
        }

        [HttpGet("logs/{userId}")]
        public async Task<IActionResult> GetLogs(int userId)
        {
            return Ok();
        }

        [HttpPut("Edit")]
        public async Task<IActionResult> EditDevice([FromBody] Device device)
        {
            return Ok();
        }
    }
}
