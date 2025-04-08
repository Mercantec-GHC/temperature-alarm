using Microsoft.AspNetCore.Mvc;
using Api.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Api.BusinessLogic;
using Api.Models.Users;

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

        //[Authorize]
        [HttpGet("get")]
        public async Task<IActionResult> ReadUser()
        {
            var claims = HttpContext.User.Claims;
            string userIdString = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            int userId = Convert.ToInt32(userIdString);
            return await _userLogic.getUser(userId);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            return await _userLogic.Login(login);
        }

        // Sends the user to userLogic
        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest user)
        {
            return await _userLogic.RegisterUser(user);
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest passwordRequest)
        {
            var claims = HttpContext.User.Claims;
            string userIdString = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            int userId = Convert.ToInt32(userIdString);
            return await _userLogic.changePassword(passwordRequest, userId);
        }


        // Sends the user and userId to userLogic
        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest userRequest)
        {
            var claims = HttpContext.User.Claims;
            string userIdString = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            int userId = Convert.ToInt32(userIdString);
            return await _userLogic.EditProfile(userRequest, userId);
        }

        // Sends the userId to userLogic
        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteUser()
        {
            var claims = HttpContext.User.Claims;
            string userIdString = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            int userId = Convert.ToInt32(userIdString);
            return await _userLogic.DeleteUser(userId);
        }

        [HttpPost("refreshToken/{refreshToken}")]
        public async Task<IActionResult> RefreshToken(string refreshToken)
        {
            return await _userLogic.RefreshToken(refreshToken);
        }

    }
}
