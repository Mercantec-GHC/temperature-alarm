using Microsoft.AspNetCore.Mvc;
using Api.Models;
using System.Security.Claims;
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
        [HttpPut("Edit")]
        public async Task<IActionResult> EditUser([FromBody] User user)
        {
            var claims = HttpContext.User.Claims;
            string userIdString = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            int userId = Convert.ToInt32(userIdString);
            return await _userLogic.EditProfile(user, userId);
        }

        // Sends the userId to userLogic
        [Authorize]
        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteUser()
        {
            var claims = HttpContext.User.Claims;
            string userIdString = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            int userId = Convert.ToInt32(userIdString);
            return await _userLogic.DeleteUser(userId);
        }

        [HttpGet("RefreshToken")]
        public async Task<IActionResult> RefreshToken(string refreshToken)
        {
            return await _userLogic.RefreshToken(refreshToken);
        }

    }
}
