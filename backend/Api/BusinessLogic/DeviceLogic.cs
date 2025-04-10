using Api.DBAccess;
using Api.Models;
using Api.Models.Devices;
using Microsoft.AspNetCore.Mvc;
using Api.AMQP;

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
            var userDetails = await _dbAccess.ReadUserDetails(userId);

            if (userDetails.Devices.Count == 0) { return new ConflictObjectResult(new { message = "Could not find any devices connected to the user" }); }


            List<GetDeviceDTO> devices = new List<GetDeviceDTO>();

            foreach (var item in userDetails.Devices)
            {
                var latestLog = item.Logs?.OrderByDescending(log => log.Date).FirstOrDefault(); // Get the latest log
                GetDeviceDTO device = new GetDeviceDTO
                {
                    Id = item.Id,
                    Name = item.Name,
                    TempHigh = item.TempHigh,
                    TempLow = item.TempLow,
                    ReferenceId = item.ReferenceId,
                    LatestLog = latestLog
                };

                devices.Add(device);
            }
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
            var user = await _dbAccess.ReadUserDetails(userId);
            var possibleDevice = _dbAccess.ReadDevice(referenceId);
            if (possibleDevice != null) { return new ConflictObjectResult(new { message = "Device with given referenceId already exists" }); }
            if (user == null) { return new ConflictObjectResult(new { message = "Could not find user" }); }

            Device device = new Device
            {
                Name = "Undefined",
                TempHigh = 100,
                TempLow = -40,
                ReferenceId = referenceId,
                Logs = new List<TemperatureLogs>(),
            };
            DeviceLimit deviceLimit = new DeviceLimit();
            deviceLimit.TempHigh = device.TempHigh;
            deviceLimit.TempLow = device.TempLow;
            deviceLimit.ReferenceId = device.ReferenceId;
            AMQPPublisher publisher = new AMQPPublisher(_configuration);
            publisher.Handle_Push_Device_Limits(deviceLimit);

            user.Devices.Add(device);

            return await _dbAccess.CreateDevice(user);
        }

        /// <summary>
        /// Checks if the deviceId matches a device
        /// </summary>
        /// <param name="device">The updated info</param>
        /// <param name="deviceId">The device to be edited</param>
        /// <returns>returns the updated device in a OkObjectResult and if there is some error it returns a ConflictObjectResult and a message that explain the reason</returns>
        public async Task<IActionResult> EditDevice(UpdateDeviceRequest request, int deviceId)
        {
            var device = await _dbAccess.ReadDevice(deviceId);
            if (device != null)
            {
                if (device.Name == "" || device.Name == null)
                    return new ConflictObjectResult(new { message = "Please enter a name" });

                device.Name = request.Name;
                device.TempLow = request.TempLow;
                device.TempHigh = request.TempHigh;
                DeviceLimit deviceLimit = new DeviceLimit();
                deviceLimit.TempHigh = device.TempHigh;
                deviceLimit.TempLow = device.TempLow;
                deviceLimit.ReferenceId = device.ReferenceId;
                AMQPPublisher publisher = new AMQPPublisher(_configuration);
                publisher.Handle_Push_Device_Limits(deviceLimit);
            }

            return await _dbAccess.UpdateDevice(device);
        }

        /// <summary>
        /// deletes a device
        /// </summary>
        /// <param name="referenceId">the id used to delete</param>
        /// <param name="userId">Used for deleting device from devices list in user</param>
        /// <returns>returns OK</returns>
        public async Task<IActionResult> DeleteDevice(int deviceId)
        {
            var device = await _dbAccess.ReadDevice(deviceId);
            if (device != null)
            {
                return await _dbAccess.DeleteDevice(device);

            }
            return new ConflictObjectResult(new { message = "Invalid user" });
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

            var logs = await _dbAccess.ReadLogs(deviceId, dateTimeRange);

            return new OkObjectResult(logs);
        }
    }
}
