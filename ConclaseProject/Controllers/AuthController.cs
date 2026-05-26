using System.Threading.Tasks;
using ConclaseProject.DTOs;
using ConclaseProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gatherly.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            var result = await _authService.RegisterAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(new { error = result.Message });
            }
            return Ok(result);
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var result = await _authService.LoginAsync(request);
            if (!result.IsSuccess)
            {
                return Unauthorized(new { error = result.Message });
            }
            return Ok(result);
        }
    }
}
