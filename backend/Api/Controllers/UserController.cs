using Microsoft.AspNetCore.Mvc;
using Models;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly DBContext _context;

        public UserController(DBContext context)
        {
            _context = context;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] User user)
        {
            DBAccess.DBAccess dBAccess = new DBAccess.DBAccess(_context);
            user = await dBAccess.Login(user);
            if (user.Id == 0) { return BadRequest(new { error = "User can't be logged in" }); }
            return Ok(user);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            DBAccess.DBAccess dBAccess = new DBAccess.DBAccess(_context);
            bool success = await dBAccess.CreateUser(user);
            if (!success) { return BadRequest(new { error = "User can't be created" }); }
            return Ok();
        }

        [HttpPut("Edit/{userId}")]
        public async Task<IActionResult> EditUser([FromBody] User user, int userId)
        {
            DBAccess.DBAccess dBAccess = new DBAccess.DBAccess(_context);
            bool success = await dBAccess.UpdateUser(user, userId);
            if (!success) { return BadRequest(new { error = "User can't be edited" }); }
            return Ok();
        }
    }
}
