using Microsoft.AspNetCore.Mvc;
using Api.Models;
using Api.DBAccess;

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
            DbAccess dBAccess = new DbAccess(_context);
            List<Device> devices = await dBAccess.ReadDevices(userId);
            if (devices.Count == 0) { return BadRequest(new { error = "There is no devices that belong to this userID" }); }
            return Ok(devices);
        }

        [HttpPost("adddevice/{userId}")]
        public async Task<IActionResult> AddDevice([FromBody] Device device, int userId)
        {
            DbAccess dBAccess = new DbAccess(_context);
            bool success = await dBAccess.CreateDevice(device, userId);
            if (!success) { return BadRequest(new { error = "This device already exist" }); }
            return Ok();
        }

        [HttpGet("logs/{deviceId}")]
        public async Task<IActionResult> GetLogs(int deviceId)
        {
            DbAccess dBAccess = new DbAccess(_context);
            List<TemperatureLogs> logs = await dBAccess.ReadLogs(deviceId);
            if (logs.Count == 0) { return BadRequest(new { error = "There is no logs that belong to this deviceId" }); }
            return Ok(logs);
        }

        [HttpPut("Edit/{deviceId}")]
        public async Task<IActionResult> EditDevice([FromBody] Device device, int deviceId)
        {
            DbAccess dBAccess = new DbAccess(_context);
            bool success = await dBAccess.UpdateDevice(device, deviceId);
            if (!success) { return BadRequest(new { error = "Device can't be edited" }); }
            return Ok();
        }
    }
}
