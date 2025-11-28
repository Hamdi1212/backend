using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Checklist.Models;
using Checklist.Helpers;
using Checklist.Models.Dtos;
using BCrypt.Net; // Add this using directive

namespace Checklist.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly JwtHelper _jwtHelper;

        public AuthController(DataContext context, IConfiguration config)
        {
            _context = context;
            _jwtHelper = new JwtHelper(config);
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginDto request)
        {
            var user = _context.Users.SingleOrDefault(u => u.Username == request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.password))
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            var token = _jwtHelper.GenerateJwtToken(user);
            return Ok(new { token = token });
        }

    }
}
