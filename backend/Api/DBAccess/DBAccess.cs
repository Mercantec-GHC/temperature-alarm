using Microsoft.EntityFrameworkCore;
using Api.Models;
using System.Text;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Api.Models.User;


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

        public async Task<User> ReadUser(int userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

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

            profile.UserName = user.UserName;

            profile.Email = user.Email;

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

        public async Task<List<Device>> ReadDevices(int userId)
        {
            var user = await _context.Users.Include(u => u.Devices).FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.Devices == null) { return new List<Device>(); }

            var devices = user.Devices;

            return devices;
        }

        public async Task<IActionResult> CreateDevice(Device device, int userId)
        {
            var user = await _context.Users.Include(u => u.Devices).FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.Devices == null) { return new ConflictObjectResult(new { message = "User did not have a device list" }); }

            if (device.Logs == null) { device.Logs = new List<TemperatureLogs>(); }

            user.Devices.Add(device);

            bool saved = await _context.SaveChangesAsync() == 1;

            if (saved) { return new OkObjectResult(saved); }

            return new ConflictObjectResult(new { message = "Could not save to database" });
        }

        public async Task<Device> ReadDevice(int deviceId)
        {
            return await _context.Devices.FirstOrDefaultAsync(d => d.Id == deviceId);
        }

        public Device ReadDevice(string refenreId)
        {
            return _context.Devices.FirstOrDefault(d => d.ReferenceId == refenreId);
        }

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

        public async Task<List<TemperatureLogs>> ReadLogs(int deviceId)
        {
            var device = await _context.Devices.Include(d => d.Logs).FirstOrDefaultAsync(d => d.Id == deviceId);

            if (device == null || device.Logs == null) { return new List<TemperatureLogs>(); }

            var logs = device.Logs;

            return logs;
        }

        public async void CreateLog(TemperatureLogs temperatureLogs, string referenceId)
        {
            var device = await _context.Devices.Include(d => d.Logs).FirstOrDefaultAsync(d => d.ReferenceId == referenceId);

            if (device == null) { return; }

            if (device.Logs == null) { device.Logs = new List<TemperatureLogs>(); }

            device.Logs.Add(temperatureLogs);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> Test()
        {
            return _context.Database.CanConnect();
        }

    }
}
