using Api.DBAccess;
using Api.Models;
using Api.Models.Devices;
using Microsoft.AspNetCore.Mvc;

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

        /// <summary>
        /// Gets the user from dbaccess using the userId and checks if the user exists
        /// Gets all devices that match the userId and checks if there are any devices connected to the user
        /// </summary>
        /// <param name="userId">UserId that matches a user that owns the devices</param>
        /// <returns>returns the devices in a OkObjectResult and if there is some error it returns a ConflictObjectResult and a message that explain the reason</returns>
        public async Task<IActionResult> GetDevices(int userId)
        {
            var profile = await _dbAccess.ReadUser(userId);

            if (profile == null) { return new ConflictObjectResult(new { message = "Could not find user" }); }

            var devices = await _dbAccess.ReadDevices(userId);

            if (devices.Count == 0) { return new ConflictObjectResult(new { message = "Could not find any devices connected to the user" }); }

            return new OkObjectResult(devices);
        }

        /// <summary>
        /// Checks if the user that the device is trying to be added to exists
        /// Then it is send to dbaccess
        /// </summary>
        /// <param name="device">The new device</param>
        /// <param name="userId">The user that owns the device</param>
        /// <returns>returns true in a OkObjectResult and if there is some error it returns a ConflictObjectResult and a message that explain the reason</returns>
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

        /// <summary>
        /// Checks if the device exist that is trying to be read from
        /// Gets the logs and checks if there are any in the list
        /// Checks if the datetimeRange have 2 values that are the same bc that means they want all logs
        /// Then it makes a new list with all data that are in the range of the 2 datetimes
        /// </summary>
        /// <param name="deviceId">The deviceId that you want from the logs</param>
        /// <returns>returns the logs in a OkObjectResult and if there is some error it returns a ConflictObjectResult and a message that explain the reason</returns>
        public async Task<IActionResult> GetLogs(DateTimeRange dateTimeRange, int deviceId)
        {
            var device = await _dbAccess.ReadDevice(deviceId);

            if (device == null) { return new ConflictObjectResult(new { message = "Could not find device" }); }

            var logs = await _dbAccess.ReadLogs(deviceId);

            if (logs.Count == 0) { return new ConflictObjectResult(new { message = "Could not find any logs connected to the device" }); }

            if (dateTimeRange.DateTimeStart == dateTimeRange.DateTimeEnd) { return new OkObjectResult(logs); }

            List<TemperatureLogs> rangedLogs = new List<TemperatureLogs>();
            foreach (var log in logs)
            {
                if (log.Date <= dateTimeRange.DateTimeStart && log.Date >= dateTimeRange.DateTimeEnd) { rangedLogs.Add(log); }
            }

            return new OkObjectResult(rangedLogs);
        }

        /// <summary>
        /// Checks if the deviceId matches a device
        /// </summary>
        /// <param name="device">The updated info</param>
        /// <param name="deviceId">The device to be edited</param>
        /// <returns>returns the updated device in a OkObjectResult and if there is some error it returns a ConflictObjectResult and a message that explain the reason</returns>
        public async Task<IActionResult> EditDevice(Device device, int deviceId)
        {
            var device1 = _dbAccess.ReadDevice(deviceId);

            if (device1 == null) { return new ConflictObjectResult(new { message = "Could not find device" }); }

            return await _dbAccess.UpdateDevice(device, deviceId);
        }
    }
}
