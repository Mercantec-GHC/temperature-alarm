﻿using Api.DBAccess;
using Api.Models;
using Api.Models.Devices;
using Api.Models.Users;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Api.BusinessLogic
{
    public class UserLogic
    {
        private readonly DbAccess _dbAccess;
        private readonly IConfiguration _configuration;

        public UserLogic(IConfiguration configuration, DbAccess dbAccess)
        {
            _dbAccess = dbAccess;
            _configuration = configuration;
        }

        public async Task<IActionResult> getUser(int userId)
        {
            User user = await _dbAccess.ReadUser(userId);

            if (user == null || user.Id == 0) { return new ConflictObjectResult(new { message = "Could not find user" }); }
            return new OkObjectResult(new { user.Id, user.UserName, user.Email });
        }

        /// <summary>
        /// First checks if the mail is a valid one with regex so if there is something before the @ and after and it has a domain
        /// Then it checks if the password is to our security standard
        /// Then it makes sure the user has a device list
        /// The last thing before it saves the user is creating a salt and then hashing of the password
        /// </summary>
        /// <param name="user">The new user</param>
        /// <returns>returns true in a OkObjectResult and if there is some error it returns a ConflictObjectResult and a message that explain the reason</returns>
        public async Task<IActionResult> RegisterUser(CreateUserRequest request)
        {
            if (!new Regex(@".+@.+\..+").IsMatch(request.Email))
            {
                return new ConflictObjectResult(new { message = "Invalid email address" });
            }

            if (!PasswordSecurity(request.Password))
            {
                return new ConflictObjectResult(new { message = "Password is not up to the security standard" });
            }

            var users = await _dbAccess.ReadAllUsers();

            foreach (var item in users)
            {
                if (item.UserName == request.UserName)
                {
                    return new ConflictObjectResult(new { message = "Username is already in use." });
                }

                if (item.Email == request.Email)
                {
                    return new ConflictObjectResult(new { message = "Email is being used already" });
                }
            }

            string salt = Guid.NewGuid().ToString();
            string hashedPassword = ComputeHash(request.Password, SHA256.Create(), salt);

            User user = new User
            {
                UserName = request.UserName,
                Email = request.Email,
                Password = hashedPassword,
                Salt = salt,
                Devices = new List<Device>()
            };
            return await _dbAccess.CreateUser(user);
        }

        /// <summary>
        /// Gets the user that matches the login
        /// Hashes the login password with the users salt
        /// checks if the hashed password that the login has is the same as the one saved in the database
        /// </summary>
        /// <param name="login">Has a username or email and a password</param>
        /// <returns>Returns a jwt token, username and userid</returns>
        public async Task<IActionResult> Login(Login login)
        {
            User user = await _dbAccess.ReadUserForLogin(login.EmailOrUsrn);

            if (user == null || user.Id == 0) { return new ConflictObjectResult(new { message = "Could not find user" }); }

            string hashedPassword = ComputeHash(login.Password, SHA256.Create(), user.Salt);

            if (user.Password == hashedPassword)
            {
                var token = GenerateJwtToken(user);
                user = await UpdateRefreshToken(user);              

                return new OkObjectResult(new { token, user.UserName, user.Id, refreshToken = user.RefreshToken });
            }

            return new ConflictObjectResult(new { message = "Invalid password" });
        }

        /// <summary>
        /// First checks if the mail is a valid one with regex so if there is something before the @ and after and it has a domain
        /// Then it checks if the password is to our security standard
        /// Finds the user that matches the userId and hashes a new hash with the old salt
        /// Then the updated user and the userId is being send to dbaccess
        /// </summary>
        /// <param name="user">Contains the updated user info</param>
        /// <param name="userId">Has the id for the user that is to be updated</param>
        /// <returns>returns the updated user in a OkObjectResult and if there is some error it returns a ConflictObjectResult and a message that explain the reason</returns>
        public async Task<IActionResult> EditProfile(UpdateUserRequest userRequest, int userId)
        {
            var profile = await _dbAccess.ReadUser(userId);
            var users = await _dbAccess.ReadAllUsers();

            if (profile == null) { return new ConflictObjectResult(new { message = "User does not exist" }); }

            foreach (var item in users)
            {
                if (item.UserName == userRequest.UserName && userId != item.Id)
                {
                    return new ConflictObjectResult(new { message = "Username is already in use." });
                }

                if (item.Email == userRequest.Email && userId != item.Id)
                {
                    return new ConflictObjectResult(new { message = "Email is being used already" });
                }
            }

            if (userRequest.Email == "" || userRequest.Email == null)
                return new ConflictObjectResult(new { message = "Please enter an email" });

            if (userRequest.UserName == "" || userRequest.UserName == null)
                return new ConflictObjectResult(new { message = "Please enter a username" });

            profile.Email = userRequest.Email;
            profile.UserName = userRequest.UserName;


            return await _dbAccess.UpdateUser(profile);
        }

        public async Task<IActionResult> changePassword(ChangePasswordRequest passwordRequest, int userId)
        {
            var user = await _dbAccess.ReadUser(userId);
            if (user == null) { return new ConflictObjectResult(new { message = "User does not exist" }); }


            string hashedPassword = ComputeHash(passwordRequest.OldPassword, SHA256.Create(), user.Salt);

            if (user.Password != hashedPassword)
            {
                return new ConflictObjectResult(new { message = "Old password is incorrect" });

            }

            if (!PasswordSecurity(passwordRequest.NewPassword))
            {
                return new ConflictObjectResult(new { message = "New password is not up to the security standard" });
            }

            string hashedNewPassword = ComputeHash(passwordRequest.NewPassword, SHA256.Create(), user.Salt);
            user.Password = hashedNewPassword;

            return await _dbAccess.UpdatePassword(user);
        }

        /// <summary>
        /// Just sends the userid of the user that is to be deleted
        /// </summary>
        /// <param name="userId">The Id of the user that is to be deleted</param>
        /// <returns>returns the true in a OkObjectResult and if there is some error it returns a ConflictObjectResult and a message that explain the reason</returns>
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var user = await _dbAccess.ReadUserDetails(userId);
            if (user != null)
            {
                return await _dbAccess.DeleteUser(user);

            }
            return new ConflictObjectResult(new { message = "Invalid user" });
        
        }

        public async Task<IActionResult> RefreshToken(string refreshToken)
        {
            User user = await _dbAccess.ReadUserByRefreshToken(refreshToken);
            if (user == null) { return new ConflictObjectResult(new { message = "Could not match refreshtoken" }); }
            user = await UpdateRefreshToken(user);
            string jwtToken = GenerateJwtToken(user);
            return new OkObjectResult(new { token = jwtToken, refreshToken = user.RefreshToken });
        }

        /// <summary>
        /// Generates a hash from a salt and input using the algorithm that is provided
        /// </summary>
        /// <param name="input">This is the input that is supposed to be hashed</param>
        /// <param name="algorithm">This is the alogorithm that is used to encrypt the input</param>
        /// <param name="salt">This is something extra added to make the hashed input more unpredictable</param>
        /// <returns>The hashed input</returns>
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

        /// <summary>
        /// Checks if password is up to our security standard
        /// </summary>
        /// <param name="password">The password that is to be checked</param>
        /// <returns>true or false dependeing on if the password is up to standard</returns>
        public bool PasswordSecurity(string password)
        {
            var hasMinimum8Chars = new Regex(@".{8,}");

            return hasMinimum8Chars.IsMatch(password);
        }

        /// <summary>
        /// Generates a JWT token that last 2 hours
        /// </summary>
        /// <param name="user">Used for sending the userid and username with the token</param>
        /// <returns>Returns a valid JWTToken</returns>
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes
            (_configuration["JwtSettings:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _configuration["JwtSettings:Issuer"],
                _configuration["JwtSettings:Audience"],
                claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<User> UpdateRefreshToken(User user)
        {
            user.RefreshToken = Guid.NewGuid().ToString();
            user.RefreshTokenExpiresAt = DateTime.Now.AddDays(30);
            await _dbAccess.UpdateUser(user);
            return user;
        }
    }
}
