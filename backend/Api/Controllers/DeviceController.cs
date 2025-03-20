using Microsoft.AspNetCore.Mvc;
using Api.Models;
using Api.DBAccess;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceController : Controller
    {
        private readonly DbAccess _dbAccess;

        public DeviceController(DbAccess dbAccess)
        {
            _dbAccess = dbAccess;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetDevices(int userId)
        {
            List<Device> devices = await _dbAccess.ReadDevices(userId);
            if (devices.Count == 0) { return BadRequest(new { error = "There is no devices that belong to this userID" }); }
            return Ok(devices);
        }

        [Authorize]
        [HttpPost("adddevice/{userId}")]
        public async Task<IActionResult> AddDevice([FromBody] Device device, int userId)
        {
            bool success = await _dbAccess.CreateDevice(device, userId);
            if (!success) { return BadRequest(new { error = "This device already exist" }); }
            return Ok();
        }

        [Authorize]
        [HttpGet("logs/{deviceId}")]
        public async Task<IActionResult> GetLogs(int deviceId)
        {
            List<TemperatureLogs> logs = await _dbAccess.ReadLogs(deviceId);
            if (logs.Count == 0) { return BadRequest(new { error = "There is no logs that belong to this deviceId" }); }
            return Ok(logs);
        }

        [Authorize]
        [HttpPut("Edit/{deviceId}")]
        public async Task<IActionResult> EditDevice([FromBody] Device device, int deviceId)
        {
            bool success = await _dbAccess.UpdateDevice(device, deviceId);
            if (!success) { return BadRequest(new { error = "Device can't be edited" }); }
            return Ok();
        }
    }
}
