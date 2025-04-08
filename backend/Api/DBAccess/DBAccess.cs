using Microsoft.EntityFrameworkCore;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Api.Models.Devices;
using Api.Models.Users;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Collections.Generic;

//All EF Core database calls
namespace Api.DBAccess
{
    public class DbAccess
    {
        private readonly DBContext _context;

        public DbAccess(DBContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Gets one user on id
        /// </summary>
        /// <param name="userId">used to get the specific user</param>
        /// <returns>returns a user object from the database based on the given id</returns>
        public async Task<User> ReadUser(int userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User> ReadUserDetails(int userId)
        {
            return await _context.Users
                .Include(u => u.Devices)
                .ThenInclude(u => u.Logs)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        // Returns a user according to refreshToken
        public async Task<User> ReadUserByRefreshToken(string refreshToken)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        }

        /// <summary>
        ///  Used to check both email and login for the login.
        /// </summary>
        /// <param name="emailOrUsername">stores the input of username or email</param>
        /// <returns>returns a user object from the database based on the given email or username</returns>
        public async Task<User> ReadUserForLogin(string emailOrUsername)
        {
            if (emailOrUsername.Contains("@"))
            {
                return await _context.Users.FirstOrDefaultAsync(u => u.Email == emailOrUsername);
            }
            else
            {
                return await _context.Users.FirstOrDefaultAsync(u => u.UserName == emailOrUsername);
            }
        }

        /// <summary>
        /// Gets all users 
        /// </summary>
        /// <returns>Return a list of users</returns>
        public async Task<List<User>> ReadAllUsers()
        {
            return await _context.Users.ToListAsync();
        }

        /// <summary>
        /// Creates a user
        /// </summary>
        /// <param name="user">Need the entire user obj</param>
        /// <returns>returns true in a OkObjectResult and if there is some error it returns a ConflictObjectResult and a message that explain the reason</returns>
        public async Task<IActionResult> CreateUser(User user)
        {
            _context.Users.Add(user);

            bool saved = await _context.SaveChangesAsync() == 1;

            if (saved) { return new OkObjectResult(true); }

            return new ConflictObjectResult(new { message = "Could not save to database" });

        }

        /// <summary>
        /// Updates the user in the database
        /// </summary>
        /// <param name="user">Contains the updated user info</param>
        /// <param name="userId">Has the id for the user that is to be updated</param>
        /// <returns>returns the updated user in a OkObjectResult and if there is some error it returns a ConflictObjectResult and a message that explain the reason</returns>
        public async Task<IActionResult> UpdateUser(User user)
        {
            _context.Entry(user).State = EntityState.Modified;

            bool saved = await _context.SaveChangesAsync() == 1;

            if (saved) { return new OkObjectResult(user);}

            return new ConflictObjectResult(new { message = "Could not save to database" });

        }

        public async Task<IActionResult> UpdatePassword(User user)
        {
            _context.Entry(user).State = EntityState.Modified;

            bool saved = await _context.SaveChangesAsync() == 1;

            if (saved) { return new OkObjectResult(user); }

            return new ConflictObjectResult(new { message = "Could not save to database" });
        }

        /// <summary>
        /// Deletes a user from the database
        /// </summary>
        /// <param name="userId">The Id of the user that is to be deleted</param>
        /// <returns>returns true in a OkObjectResult and if there is some error it returns a ConflictObjectResult and a message that explain the reason</returns>
        public async Task<IActionResult> DeleteUser(User user)
        {
            _context.Users.Remove(user);
            bool saved = await _context.SaveChangesAsync() >= 0;

            if (saved) { return new OkObjectResult(saved); }

            return new ConflictObjectResult(new { message = "Could not save to database" });
        }

        // Returns all devices
        public List<Device> ReadDevices()
        {
            return _context.Devices.ToList();
        }

        // Returns a device according to deviceId
        public async Task<Device> ReadDevice(int deviceId)
        {
            return await _context.Devices.FirstOrDefaultAsync(d => d.Id == deviceId);
        }

        // Returns a device according to referenceId
        public Device ReadDevice(string referenceId)
        {
            return _context.Devices.FirstOrDefault(d => d.ReferenceId == referenceId);
        }

        /// <summary>
        /// Creates a user using entityframework core
        /// </summary>
        /// <param name="device">The device that is going to be created</param>
        /// <param name="userId">The user that owns the device</param>
        /// <returns>returns the true in a OkObjectResult and if there is some error it returns a ConflictObjectResult and a message that explain the reason</returns>
        public async Task<IActionResult> CreateDevice(User user)
        {
            _context.Entry(user).State = EntityState.Modified;

            bool saved = await _context.SaveChangesAsync() == 2;

            if (saved) { return new OkObjectResult(user.Id); }

            return new ConflictObjectResult(new { message = "Could not save to database" });
        }

        /// <summary>
        /// Updates a device in the database
        /// </summary>
        /// <param name="request">Contains the updated device info</param>
        /// <param name="referenceId">Has the id for the device that is to be updated</param>
        /// <returns>returns the updated device in a OkObjectResult and if there is some error it returns a ConflictObjectResult and a message that explain the reason</returns>
        public async Task<IActionResult> UpdateDevice(Device device)
        {
            _context.Entry(device).State = EntityState.Modified;
    
            bool saved = await _context.SaveChangesAsync() == 1;

            if (saved) { return new OkObjectResult(device); }

            return new ConflictObjectResult(new { message = "Could not save to database" });
        }

        public async Task<IActionResult> DeleteDevice(Device device)
        {
            _context.Devices.Remove(device);
            bool saved = await _context.SaveChangesAsync() >= 0;

            if (saved) { return new OkObjectResult(saved); }

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
