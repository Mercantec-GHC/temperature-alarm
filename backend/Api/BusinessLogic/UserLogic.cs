using Api.DBAccess;
using Api.Models;
using Api.Models.User;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

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
            User user = await _dbAccess.getUser(userId);

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
        public async Task<IActionResult> RegisterUser(User user)
        {
            if (!new Regex(@".+@.+\..+").IsMatch(user.Email))
            {
                return new ConflictObjectResult(new { message = "Invalid email address" });
            }

            if (!PasswordSecurity(user.Password))
            {
                return new ConflictObjectResult(new { message = "Password is not up to the security standard" });
            }

            if (user.Devices == null)
            {
                user.Devices = new List<Device>();
            }

            string salt = Guid.NewGuid().ToString();
            string hashedPassword = ComputeHash(user.Password, SHA256.Create(), salt);

            user.Salt = salt;
            user.Password = hashedPassword;

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
            User user = await _dbAccess.Login(login);

            if (user == null || user.Id == 0) { return new ConflictObjectResult(new { message = "Could not find user" }); }

            string hashedPassword = ComputeHash(login.Password, SHA256.Create(), user.Salt);

            if (user.Password == hashedPassword)
            {
                var token = GenerateJwtToken(user);
                user.RefreshToken = Guid.NewGuid().ToString();
                _dbAccess.UpdatesRefreshToken(user.RefreshToken, user.Id);
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
        public async Task<IActionResult> EditProfile(User user, int userId)
        {
            return await _dbAccess.UpdateUser(userRequest, userId);
        }

        public async Task<IActionResult> changePassword(ChangePasswordRequest passwordRequest, int userId)
        {
            var user = await _dbAccess.ReadUser(userId);

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

            return await _dbAccess.updatePassword(hashedNewPassword, userId);
        }

        /// <summary>
        /// Just sends the userid of the user that is to be deleted
        /// </summary>
        /// <param name="userId">The Id of the user that is to be deleted</param>
        /// <returns>returns the true in a OkObjectResult and if there is some error it returns a ConflictObjectResult and a message that explain the reason</returns>
        public async Task<IActionResult> DeleteUser(int userId)
        {
            return await _dbAccess.DeleteUser(userId);
        }

        public async Task<IActionResult> RefreshToken(string refreshToken)
        {
            User user = await _dbAccess.ReadUser(refreshToken);
            if (user == null) { return new ConflictObjectResult(new { message = "Could not match refreshtoken" }); }
            return new OkObjectResult(GenerateJwtToken(user));
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
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
