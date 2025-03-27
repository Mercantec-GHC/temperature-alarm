using Api.DBAccess;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Api.BusinessLogic
{
    public class DeviceLogic
    {
        private readonly DbAccess _dbAccess;
        private readonly IConfiguration _configuration;

        public DeviceLogic(IConfiguration configuration, DbAccess dbAccess)
        {
            _dbAccess = dbAccess;
            _configuration = configuration;
        }

        public async Task<IActionResult> GetDevices(int userId)
        {
            var profile = await _dbAccess.ReadUser(userId);

            if (profile == null) { return new ConflictObjectResult(new { message = "Could not find user" }); }

            var devices = await _dbAccess.ReadDevices(userId);

            if (devices.Count == 0) { return new ConflictObjectResult(new { message = "Could not find any devices connected to the user" }); }

            return new OkObjectResult(devices);
        }

        public async Task<IActionResult> AddDevice(string referenceId, int userId)
        {
            var profile = await _dbAccess.ReadUser(userId);

            if (profile == null) { return new ConflictObjectResult(new { message = "Could not find user" }); }

            Device device = new Device
            {
                Name = "Undefined",
                TempHigh = 0,
                TempLow = 0,
                ReferenceId = referenceId,
                Logs = new List<TemperatureLogs>(),
            };

            return await _dbAccess.CreateDevice(device, userId);
        }

        public async Task<IActionResult> GetLogs(int deviceId)
        {
            var device = await _dbAccess.ReadDevice(deviceId);

            if (device == null) { return new ConflictObjectResult(new { message = "Could not find device" }); }

            var logs = await _dbAccess.ReadLogs(deviceId);

            if (logs.Count == 0) { return new ConflictObjectResult(new { message = "Could not find any logs connected to the device" }); }

            return new OkObjectResult(logs);
        }

        public async Task<IActionResult> EditDevice(Device device, int deviceId)
        {
            var device1 = _dbAccess.ReadDevice(deviceId);

            if (device1 == null) { return new ConflictObjectResult(new { message = "Could not find device" }); }

            return await _dbAccess.UpdateDevice(device, deviceId);
        }
    }
}
