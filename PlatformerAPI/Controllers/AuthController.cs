using Microsoft.AspNetCore.Mvc;
using PlatformerAPI.Data;
using PlatformerAPI.Models;
using PlatformerAPI.Services;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace PlatformerAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;

        public AuthController(AppDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            Console.WriteLine($"Received login request for username: {request.Username}");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
            {
                return Unauthorized("User not found");
            }

            try
            {
                bool verified = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
                if (!verified)
                {
                    return Unauthorized("Invalid password");
                }
            }
            catch (Exception ex)
            {
                // Логируем проблему с хэшированием
                Console.WriteLine($"Error verifying password: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

            var token = _authService.GenerateToken(user.Username);
            return Ok(new { Token = token });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest("User already exists");
            }

            var hashedPass = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                PasswordHash = hashedPass
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
