using Microsoft.AspNetCore.Mvc;
using Api.Models;
using Api.DBAccess;
using Microsoft.AspNetCore.Authorization;
using Api.BusinessLogic;
using System.Security.Claims;

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
        [HttpPost("adddevice")]
        public async Task<IActionResult> AddDevice([FromBody] Device device)
        {
            var claims = HttpContext.User.Claims;
            string userIdString = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            int userId = Convert.ToInt32(userIdString);
            return await _deviceLogic.AddDevice(device, userId);
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
                dateTimeRange.DateTimeStart = DateTime.Now;
                dateTimeRange.DateTimeEnd = dateTimeRange.DateTimeStart;
            }
            return await _deviceLogic.GetLogs(dateTimeRange, deviceId);
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
