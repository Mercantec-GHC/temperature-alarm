using Microsoft.EntityFrameworkCore;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Api.Models.Devices;
using Api.Models.Users;


namespace Api.DBAccess
{
    public class DbAccess
    {
        private readonly DBContext _context;

        public DbAccess(DBContext context)
        {
            _context = context;
        }

        public async Task<User> getUser(int userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        /// <summary>
        /// Creates a user using entityframework core
        /// </summary>
        /// <param name="user">Need the entire user obj</param>
        /// <returns>returns true in a OkObjectResult and if there is some error it returns a ConflictObjectResult and a message that explain the reason</returns>
        public async Task<IActionResult> CreateUser(User user)
        {
            var users = await _context.Users.ToListAsync();

            foreach (var item in users)
            {
                if (item.UserName == user.UserName)
                {
                    return new ConflictObjectResult(new { message = "Username is already in use." });
                }

                if (item.Email == user.Email)
                {
                    return new ConflictObjectResult(new { message = "Email is being used already" });
                }
            }

            _context.Users.Add(user);
            bool saved = await _context.SaveChangesAsync() == 1;

            if (saved) { return new OkObjectResult(true); }

            return new ConflictObjectResult(new { message = "Could not save to databse" });
        }

        /// <summary>
        /// Returns a user that matches either the email or username
        /// </summary>
        /// <param name="login">Has a username or email and a password here the password is not used</param>
        /// <returns>(user) that matches the login</returns>
        public async Task<User> Login(Login login)
        {
            User user = new User();
            if (!login.EmailOrUsrn.Contains("@"))
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == login.EmailOrUsrn);
            }
            else
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.Email == login.EmailOrUsrn);
            }

            if (user == null || user.Id == 0) { return new User(); }
            return user;
        }

        // Returns a user according to userID
        public async Task<User> ReadUser(int userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        // Returns a user according to refreshToken
        public async Task<User> ReadUser(string refreshToken)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        }

        // Updates the refreshtoken saved in DB
        public async void UpdatesRefreshToken(string refreshToken, int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiresAt = DateTime.Now.AddDays(7);
        }

        /// <summary>
        /// Updates the user in the database
        /// </summary>
        /// <param name="user">Contains the updated user info</param>
        /// <param name="userId">Has the id for the user that is to be updated</param>
        /// <returns>returns the updated user in a OkObjectResult and if there is some error it returns a ConflictObjectResult and a message that explain the reason</returns>
        public async Task<IActionResult> UpdateUser(EditUserRequest user, int userId)
        {
            var profile = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var users = await _context.Users.ToListAsync();

            if (profile == null) { return new ConflictObjectResult(new { message = "User does not exist" }); }

            foreach (var item in users)
            {
                if (item.UserName == user.UserName && userId != item.Id)
                {
                    return new ConflictObjectResult(new { message = "Username is already in use." });
                }

                if (item.Email == user.Email && userId != item.Id)
                {
                    return new ConflictObjectResult(new { message = "Email is being used already" });
                }
            }

            if(user.Email == "" || user.Email == null)
                return new ConflictObjectResult(new { message = "Please enter an email" });

            if (user.UserName == "" || user.UserName == null)
                return new ConflictObjectResult(new { message = "Please enter a username" });

            profile.Email = user.Email;
            profile.UserName = user.UserName;



            bool saved = await _context.SaveChangesAsync() == 1;

            if (saved) { return new OkObjectResult(profile); }

            return new ConflictObjectResult(new { message = "Could not save to database" });
        }

        public async Task<IActionResult> updatePassword(string newPassword, int userId)
        {
            var profile = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (profile == null) { return new ConflictObjectResult(new { message = "User does not exist" }); }

            profile.Password = newPassword;

            bool saved = await _context.SaveChangesAsync() == 1;

            if (saved) { return new OkObjectResult(profile); }

            return new ConflictObjectResult(new { message = "Could not save to database" });
        }

        /// <summary>
        /// Deletes a user from the database
        /// </summary>
        /// <param name="userId">The Id of the user that is to be deleted</param>
        /// <returns>returns true in a OkObjectResult and if there is some error it returns a ConflictObjectResult and a message that explain the reason</returns>
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var user = await _context.Users.Include(u => u.Devices).FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null)
            {
                if (user.Devices != null && user.Devices.Count > 0)
                {
                    foreach (var item in user.Devices)
                    {
                        var device = await _context.Devices.Include(d => d.Logs).FirstOrDefaultAsync(d => d.Id == item.Id);
                        if (device != null) { _context.Devices.Remove(device); }
                    }
                }
                _context.Users.Remove(user);
                bool saved = await _context.SaveChangesAsync() >= 0;

                if (saved) { return new OkObjectResult(saved); }

                return new ConflictObjectResult(new { message = "Could not save to database" });
            }
            return new ConflictObjectResult(new { message = "Invalid user" });
        }

        // Returns devices according to userID
        public async Task<List<Device>> ReadDevices(int userId)
        {
            var user = await _context.Users.Include(u => u.Devices).FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.Devices == null) { return new List<Device>(); }

            var devices = user.Devices;

            return devices;
        }

        /// <summary>
        /// Creates a user using entityframework core
        /// </summary>
        /// <param name="device">The device that is going to be created</param>
        /// <param name="userId">The user that owns the device</param>
        /// <returns>returns the true in a OkObjectResult and if there is some error it returns a ConflictObjectResult and a message that explain the reason</returns>
        public async Task<IActionResult> CreateDevice(Device device, int userId)
        {
            var user = await _context.Users.Include(u => u.Devices).FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.Devices == null) { return new ConflictObjectResult(new { message = "User did not have a device list" }); }

            user.Devices.Add(device);

            bool saved = await _context.SaveChangesAsync() == 1;

            if (saved) { return new OkObjectResult(saved); }

            return new ConflictObjectResult(new { message = "Could not save to database" });
        }

        // Returns a device according to deviceId
        public async Task<Device> ReadDevice(int deviceId)
        {
            return await _context.Devices.FirstOrDefaultAsync(d => d.Id == deviceId);
        }

        // Returns a device according to refenreId
        public Device ReadDevice(string referenceId)
        {
            return _context.Devices.FirstOrDefault(d => d.ReferenceId == referenceId);
        }

        public async Task<IActionResult> EditDevice(EditDeviceRequest request, string referenceId)
        {
            var device = await _context.Devices.FirstOrDefaultAsync(d => d.ReferenceId == referenceId);
            if (device != null)
            {
                if (device.Name == "" || device.Name == null)
                    return new ConflictObjectResult(new { message = "Please enter a name" });

                device.Name = request.Name;
                device.TempLow = request.TempLow;
                device.TempHigh = request.TempHigh;

                bool saved = await _context.SaveChangesAsync() >= 0;

                if (saved) { return new OkObjectResult(saved); }

                return new ConflictObjectResult(new { message = "Could not save to database" });
            }
            return new ConflictObjectResult(new { message = "Invalid device. May already be deleted" });
        }

        public async Task<IActionResult> DeleteDevice(string referenceId, int userId)
        {
            var user = await _context.Users
                .Include(u => u.Devices)  // Ensure devices are loaded
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return new NotFoundObjectResult(new { message = "User not found" });
            }

            var device = user.Devices?.FirstOrDefault(d => d.ReferenceId == referenceId);

            if (device != null || user.Devices != null)
            {
                user.Devices.Remove(device);
                _context.Devices.Remove(device);
                bool saved = await _context.SaveChangesAsync() > 0;

                if (saved) return new OkObjectResult(new { message = "Device deleted successfully" });

                return new ConflictObjectResult(new { message = "Could not save to database" });
            }

            return new NotFoundObjectResult(new { message = "Device not found or already deleted" });
        }

        // Returns all devices
        public List<Device> ReadDevices()
        {
            return _context.Devices.ToList();
        }

        /// <summary>
        /// Updates a device in the database
        /// </summary>
        /// <param name="device">Contains the updated device info</param>
        /// <param name="deviceId">Has the id for the device that is to be updated</param>
        /// <returns>returns the updated device in a OkObjectResult and if there is some error it returns a ConflictObjectResult and a message that explain the reason</returns>
        public async Task<IActionResult> UpdateDevice(Device device, int deviceId)
        {
            var device1 = await _context.Devices.FirstOrDefaultAsync(u => u.Id == deviceId);

            if (device1 == null) { return new ConflictObjectResult(new { message = "Device does not exist" }); }

            device1.TempLow = device.TempLow;

            device1.TempHigh = device.TempHigh;

            device1.ReferenceId = device.ReferenceId;

            device1.Name = device.Name;

            bool saved = await _context.SaveChangesAsync() == 1;

            if (saved) { return new OkObjectResult(device1); }

            return new ConflictObjectResult(new { message = "Could not save to database" });
        }

        /// <summary>
        /// Returns the logs from the device
        /// </summary>
        /// <param name="deviceId">Has the id for the device that the los belong too</param>
        /// <param name="range">Return only logs within the specified datetime range</param>
        /// <returns></returns>
        public async Task<List<TemperatureLogs>> ReadLogs(int deviceId, DateTimeRange range)
        {
            return _context.Devices.Include(d => d.Logs.Where(l => l.Date > range.DateTimeStart && l.Date < range.DateTimeEnd)).Where(d => d.Id == deviceId).FirstOrDefault().Logs;
        }

        /// <summary>
        /// Creates a new log
        /// </summary>
        /// <param name="temperatureLogs">the new log</param>
        /// <param name="referenceId">the referenceId that belongs too the device that recoded the log</param>
        public async void CreateLog(TemperatureLogs temperatureLogs, string referenceId)
        {
            var device = await _context.Devices.Include(d => d.Logs).FirstOrDefaultAsync(d => d.ReferenceId == referenceId);

            if (device == null) { return; }

            if (device.Logs == null) { device.Logs = new List<TemperatureLogs>(); }

            device.Logs.Add(temperatureLogs);
            await _context.SaveChangesAsync();
        }

        // Does a health check on the database access
        public async Task<bool> Test()
        {
            return _context.Database.CanConnect();
        }

    }
}
