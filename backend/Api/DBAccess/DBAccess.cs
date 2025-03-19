using Microsoft.EntityFrameworkCore;
using Models;

namespace Api.DBAccess
{
    public class DBAccess
    {
        private readonly DBContext _context;

        public DBAccess(DBContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateUser(User user)
        {
            var users = await _context.Users.ToListAsync();

            foreach (var item in users)
            {
                if (item.UserName == user.UserName || item.Email == user.Email)
                {
                    return false;
                }
            }
            if (user.Devices == null)
            {
                user.Devices = new List<Device>();
            }

            _context.Users.Add(user);
            return await _context.SaveChangesAsync() == 1;
        }

        public async Task<User> Login(User user)
        {
            var profile = await _context.Users.FirstAsync(u => u.UserName == user.UserName);

            if (profile.Password == user.Password)
            {
                profile.Password = "";
                return profile;
            }
            return new User();
        }

        public async Task<bool> UpdateUser(User user, int userId)
        {
            var profile = await _context.Users.FirstAsync(u => u.Id == userId);

            profile.UserName = user.UserName;

            profile.Email = user.Email;

            profile.Password = user.Password;

            return await _context.SaveChangesAsync() == 1;
        }

        public async Task<List<Device>> ReadDevices(int userId)
        {
            var user = await _context.Users.Include(u => u.Devices).FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.Devices == null) { return new List<Device>(); }

            var devices = user.Devices;

            return devices;
        }

        public async Task<bool> CreateDevice(Device device, int userId)
        {
            var user = await _context.Users.Include(u => u.Devices).FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.Devices == null) { return false; }

            if (device.Logs == null) { device.Logs = new List<TemperatureLogs>(); }

            user.Devices.Add(device);

            return await _context.SaveChangesAsync() == 1;
        }

        public async Task<List<TemperatureLogs>> ReadLogs(int deviceId)
        {
            var device = await _context.Devices.Include(d => d.Logs).FirstOrDefaultAsync(d => d.Id == deviceId);

            if (device == null || device.Logs == null) { return new List<TemperatureLogs>(); }

            var logs = device.Logs;

            return logs;
        }

        public async Task<bool> UpdateDevice(Device device, int deviceId)
        {
            var device1 = await _context.Devices.FirstAsync(u => u.Id == deviceId);

            device1.TempLow = device.TempLow;

            device1.TempHigh = device.TempHigh;

            device1.ReferenceId = device.ReferenceId;

            return await _context.SaveChangesAsync() == 1;
        }
    }
}
