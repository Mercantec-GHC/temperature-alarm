using Microsoft.AspNetCore.Mvc;
using Api.Models;
using Api.DBAccess;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : Controller
    {
        private readonly DBContext _context;
        private readonly IConfiguration _configuration;

        public UserController(DBContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] User user)
        {
            DbAccess dBAccess = new DbAccess(_context);
            user = await dBAccess.Login(user);
            if (user.Id == 0) { return Unauthorized(new { error = "Invalid username or password" }); }
            var token = GenerateJwtToken(user);
            return Ok(new { token, user.UserName, user.Id });
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            DbAccess dBAccess = new DbAccess(_context);
            bool success = await dBAccess.CreateUser(user);
            if (!success) { return BadRequest(new { error = "User can't be created" }); }
            return Ok();
        }

        [Authorize]
        [HttpPut("Edit/{userId}")]
        public async Task<IActionResult> EditUser([FromBody] User user, int userId)
        {
            DbAccess dBAccess = new DbAccess(_context);
            bool success = await dBAccess.UpdateUser(user, userId);
            if (!success) { return BadRequest(new { error = "User can't be edited" }); }
            return Ok();
        }

        [Authorize]
        [HttpDelete("Delete/{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            DbAccess dbAccess = new DbAccess(_context);
            bool success = await dbAccess.DeleteUser(userId);
            if (!success) { return BadRequest(new { error = "User can't be deleted" }); }
            return Ok();
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
