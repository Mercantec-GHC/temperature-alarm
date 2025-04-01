using Microsoft.AspNetCore.Mvc;
using Api.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Api.BusinessLogic;
using Api.Models.User;

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
        [HttpGet("Get")]
        public async Task<IActionResult> GetUSer()
        {
            var claims = HttpContext.User.Claims;
            string userIdString = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            int userId = Convert.ToInt32(userIdString);
            return await _userLogic.getUser(userId);
        }

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

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> changePassword([FromBody] ChangePasswordRequest passwordRequest)
        {
            var claims = HttpContext.User.Claims;
            string userIdString = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            int userId = Convert.ToInt32(userIdString);
            return await _userLogic.changePassword(passwordRequest, userId);
        }


        // Sends the user and userId to userLogic
        [Authorize]
        [HttpPut("Update")]
        public async Task<IActionResult> EditUser([FromBody] EditUserRequest userRequest)
        {
            var claims = HttpContext.User.Claims;
            string userIdString = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            int userId = Convert.ToInt32(userIdString);
            return await _userLogic.EditProfile(userRequest, userId);
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

    }
}
