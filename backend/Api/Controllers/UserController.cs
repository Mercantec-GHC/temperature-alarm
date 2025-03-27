using Microsoft.AspNetCore.Mvc;
using Api.Models;
using Api.DBAccess;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Api.BusinessLogic;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly UserLogic _userLogic;

        public UserController(UserLogic userLogic)
        {
            _userLogic = userLogic;
        }

        // Sends the login to userLogic
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            return await _userLogic.Login(login);
        }

        // Sends the user to userLogic
        [HttpPost("Create")]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            return await _userLogic.RegisterUser(user);
        }

        // Sends the user and userId to userLogic
        [Authorize]
        [HttpPut("Edit/{userId}")]
        public async Task<IActionResult> EditUser([FromBody] User user, int userId)
        {
            return await _userLogic.EditProfile(user, userId);
        }

        // Sends the userId to userLogic
        [Authorize]
        [HttpDelete("Delete/{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            return await _userLogic.DeleteUser(userId);
        }

        [HttpGet("RefreshToken")]
        public async Task<IActionResult> RefreshToken(string refreshToken)
        {
            return await _userLogic.RefreshToken(refreshToken);
        }

    }
}
