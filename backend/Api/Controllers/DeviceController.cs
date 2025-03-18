using Microsoft.AspNetCore.Mvc;
using Models;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceController : Controller
    {
        private readonly DBContext _context;

        public DeviceController(DBContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> GetDevices(int userId)
        {
            DBAccess.DBAccess dBAccess = new DBAccess.DBAccess(_context);
            List<Device> devices = await dBAccess.GetDevices(userId);
            if (devices.Count == 0) { return BadRequest(new { error = "There is no devices that belong to this userID" }); }
            return Ok(devices);
        }

        [HttpPost("adddevice/{userId}")]
        public async Task<IActionResult> AddDevice([FromBody] Device device, int userId)
        {
            DBAccess.DBAccess dBAccess = new DBAccess.DBAccess(_context);
            bool success = await dBAccess.AddDevice(device, userId);
            if (!success) { return BadRequest(new { error = "This device already exist" }); }
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
