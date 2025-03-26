using Microsoft.AspNetCore.Mvc;
using Api.Models;
using Api.DBAccess;
using Microsoft.AspNetCore.Authorization;
using Api.BusinessLogic;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceController : Controller
    {
        private readonly DbAccess _dbAccess;
        private readonly DeviceLogic _deviceLogic;

        public DeviceController(DbAccess dbAccess, DeviceLogic deviceLogic)
        {
            _dbAccess = dbAccess;
            _deviceLogic = deviceLogic;
        }

        // Sends the userId to deviceLogic
        [Authorize]
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetDevices(int userId)
        {
            return await _deviceLogic.GetDevices(userId);
        }

        // Sends the device and userId to deviceLogic
        [Authorize]
        [HttpPost("adddevice/{userId}")]
        public async Task<IActionResult> AddDevice([FromBody] Device device, int userId)
        {
            return await _deviceLogic.AddDevice(device, userId);
        }

        // Sends the deviceId to deviceLogic
        [Authorize]
        [HttpGet("logs/{deviceId}")]
        public async Task<IActionResult> GetLogs(int deviceId)
        {
            return await _deviceLogic.GetLogs(deviceId);
        }

        // Sends the deviceId to deviceLogic
        [Authorize]
        [HttpPut("Edit/{deviceId}")]
        public async Task<IActionResult> EditDevice([FromBody] Device device, int deviceId)
        {
            return await _deviceLogic.EditDevice(device, deviceId);
        }
    }
}
