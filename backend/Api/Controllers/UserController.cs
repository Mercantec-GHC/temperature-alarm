using Microsoft.AspNetCore.Mvc;
using Api.Models;
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
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUSer(int userId)
        {
            return await _userLogic.getUser(userId);
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            return await _userLogic.Login(login);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            return await _userLogic.RegisterUser(user);
        }

        //[Authorize]
        [HttpPut("edit/{userId}")]
        public async Task<IActionResult> EditUser([FromBody] EditUserRequest userRequest, int userId)
        {
            return await _userLogic.EditProfile(userRequest, userId);
        }

        //[Authorize]
        [HttpPut("change-password/{userId}")]
        public async Task<IActionResult> changePassword([FromBody] ChangePasswordRequest passwordRequest, int userId)
        {
            return await _userLogic.changePassword(passwordRequest, userId);
        }

        [Authorize]
        [HttpDelete("delete/{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            return await _userLogic.DeleteUser(userId);
        }
    }
}
