using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Health() { return Ok(); }
    }
}
