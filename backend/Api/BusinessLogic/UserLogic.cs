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
        public async Task<IActionResult> Login(Login login)
        {
            User user = await _dbAccess.Login(login);

            if (user == null || user.Id == 0) { return new ConflictObjectResult(new { message = "Could not find user" }); }

            string hashedPassword = ComputeHash(login.Password, SHA256.Create(), user.Salt);

            if (user.Password == hashedPassword)
            {
                var token = GenerateJwtToken(user);
                return new OkObjectResult(new { token, user.Id});
            }

            return new ConflictObjectResult(new { message = "Invalid password" });
        }

        public async Task<IActionResult> EditProfile(EditUserRequest userRequest, int userId)
        {
            if (!new Regex(@".+@.+\..+").IsMatch(userRequest.Email))
            {
                return new ConflictObjectResult(new { message = "Invalid email address" });
            }


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

        public async Task<IActionResult> DeleteUser(int userId)
        {
            return await _dbAccess.DeleteUser(userId);
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

        public bool PasswordSecurity(string password)
        {
            var hasMinimum8Chars = new Regex(@".{8,}");

            return hasMinimum8Chars.IsMatch(password);
        }

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
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
