using Microsoft.AspNetCore.Mvc;
using Api.Models;
using Api.DBAccess;
using Microsoft.AspNetCore.Authorization;
using Api.BusinessLogic;
using System.Security.Claims;
using Api.Models.Devices;

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
        [HttpGet]
        public async Task<IActionResult> GetDevices()
        {
            var claims = HttpContext.User.Claims;
            string userIdString = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            int userId = Convert.ToInt32(userIdString);
            return await _deviceLogic.GetDevices(userId);
        }

        // Sends the device and userId to deviceLogic
        [Authorize]
        [HttpPost("adddevice/{referenceId}")]
        public async Task<IActionResult> AddDevice(string referenceId)
        {
            var claims = HttpContext.User.Claims;
            string userIdString = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            int userId = Convert.ToInt32(userIdString);
            return await _deviceLogic.AddDevice(referenceId, userId);
        }

        // Sends the deviceId to deviceLogic
        [Authorize]
        [HttpGet("logs/{deviceId}")]
        public async Task<IActionResult> GetLogs(int deviceId, DateTime? dateTimeStart = null, DateTime? dateTimeEnd = null)
        {
            DateTimeRange dateTimeRange = new DateTimeRange();
            if (dateTimeStart != null && dateTimeEnd != null)
            {
                dateTimeRange.DateTimeStart = (DateTime)dateTimeStart;
                dateTimeRange.DateTimeEnd= (DateTime)dateTimeEnd;
            }
            else
            {
                dateTimeRange.DateTimeStart = DateTime.UnixEpoch;
                dateTimeRange.DateTimeEnd = DateTime.Now;
            }
            return await _deviceLogic.GetLogs(dateTimeRange, deviceId);
        }

        // Sends the deviceId to deviceLogic
        [Authorize]
        [HttpPut("update/{deviceId}")]
        public async Task<IActionResult> EditDevice([FromBody] EditDeviceRequest device, int deviceId)
        {
            return await _deviceLogic.EditDevice(device, deviceId);
        }

        // Sends the userId to userLogic
        [Authorize]
        [HttpDelete("Delete/{deviceId}")]
        public async Task<IActionResult> DeleteDevice(int deviceId)
        {
            var claims = HttpContext.User.Claims;
            string userIdString = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            int userId = Convert.ToInt32(userIdString);
            return await _deviceLogic.DeleteDevice(deviceId, userId);
        }
    }
}
