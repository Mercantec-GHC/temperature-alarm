﻿using Microsoft.EntityFrameworkCore;
using Api.Models;
using System.Text;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;


namespace Api.DBAccess
{
    public class DbAccess
    {
        private readonly DBContext _context;

        public DbAccess(DBContext context)
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
            string salt = Guid.NewGuid().ToString();
            string hashedPassword = ComputeHash(user.Password, SHA256.Create(), salt);

            user.Salt = salt;
            user.Password = hashedPassword;

            _context.Users.Add(user);
            return await _context.SaveChangesAsync() == 1;
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

            if (user == null) { return new User(); }

            string hashedPassword = ComputeHash(user.Password, SHA256.Create(), user.Salt);

            if (hashedPassword == user.Password)
            {
                return user;
            }
            return new User();
        }

        public async Task<bool> UpdateUser(User user, int userId)
        {
            var profile = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (profile == null) { return false; }

            profile.UserName = user.UserName;

            profile.Email = user.Email;

            profile.Password = user.Password;

            return await _context.SaveChangesAsync() == 1;
        }

        public async Task<bool> DeleteUser(int userId)
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
                return await _context.SaveChangesAsync() == 1;
            }
            return false;
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

        public async Task<bool> UpdateDevice(Device device, int deviceId)
        {
            var device1 = await _context.Devices.FirstOrDefaultAsync(u => u.Id == deviceId);

            device1.TempLow = device.TempLow;

            device1.TempHigh = device.TempHigh;

            device1.ReferenceId = device.ReferenceId;

            device1.Name = device.Name;

            return await _context.SaveChangesAsync() == 1;
        }

        public async Task<List<TemperatureLogs>> ReadLogs(int deviceId)
        {
            var device = await _context.Devices.Include(d => d.Logs).FirstOrDefaultAsync(d => d.Id == deviceId);

            if (device == null || device.Logs == null) { return new List<TemperatureLogs>(); }

            var logs = device.Logs;

            return logs;
        }

        public async Task<bool> Test()
        {
            return _context.Database.CanConnect();
        }

        private static string ComputeHash(string input, HashAlgorithm algorithm, string salt)
        {
            Byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            Byte[] saltBytes = Encoding.UTF8.GetBytes(salt);

            // Combine salt and input bytes
            Byte[] saltedInput = new Byte[saltBytes.Length + inputBytes.Length];
            saltBytes.CopyTo(saltedInput, 0);
            inputBytes.CopyTo(saltedInput, saltBytes.Length);

            Byte[] hashedBytes = algorithm.ComputeHash(saltedInput);

            return BitConverter.ToString(hashedBytes);
        }
    }
}
