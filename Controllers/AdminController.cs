using Checklist.Models;
using Microsoft.EntityFrameworkCore;
using Checklist.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Checklist.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly DataContext _context;

        public AdminController(DataContext context)
        {
            _context = context;
        }

        [HttpPost("createUser")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return BadRequest(new { message = "Username already exists" });

            var hashed = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username,
                password = hashed,
                FullName = dto.FullName,
                Role = dto.Role ?? "User"
            };

            _context.Users.Add(user);

            // Assign projects if provided
            if (dto.ProjectIds is { Count: > 0 })
            {
                // Validate project IDs exist without loading full entities
                var validProjectIds = await _context.Projects
                    .Where(p => dto.ProjectIds.Contains(p.Id))
                    .Select(p => p.Id)
                    .ToListAsync();

                foreach (var projectId in validProjectIds)
                {
                    _context.UserProjects.Add(new UserProject
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        ProjectId = projectId
                    });
                }

                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                user.Id,
                user.Username,
                user.FullName,
                user.Role,
                AssignedProjects = dto.ProjectIds
            });
        }
        // Add this inside AdminController
        [HttpGet("getAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new {
                    Id = u.Id,
                    Username = u.Username,
                    FullName = u.FullName,
                    Role = u.Role
                })
                .ToListAsync();

            return Ok(users);
        }
        [HttpDelete("deleteUser/{id:guid}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = "User not found" });

            // optional: prevent deleting the last admin — check role counts
            if (user.Role == "Admin")
            {
                var adminCount = await _context.Users.CountAsync(u => u.Role == "Admin");
                if (adminCount <= 1)
                    return BadRequest(new { message = "Cannot delete the last admin." });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "User deleted" });
        }
        [HttpPut("updateUser/{id:guid}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UserUpdateDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            // Update user basic info
            user.FullName = dto.FullName;
            user.Role = dto.Role;

            // Update password only if provided
            if (!string.IsNullOrEmpty(dto.Password))
            {
                user.password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            // Update assigned projects
            if (dto.ProjectIds != null)
            {
                // Remove old project assignments
                var existing = _context.UserProjects.Where(up => up.UserId == user.Id);
                _context.UserProjects.RemoveRange(existing);

                // Add new project assignments
                foreach (var projectId in dto.ProjectIds)
                {
                    _context.UserProjects.Add(new UserProject
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        ProjectId = projectId
                    });
                }
            }


            await _context.SaveChangesAsync();
            return Ok(new { message = "User updated successfully" });
        }
        [HttpGet("getUserDetails/{id:guid}")]
        public async Task<IActionResult> GetUserDetails(Guid id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound(new { message = "User not found" });

            // Get projects assigned to the user
            var projects = await _context.UserProjects
                .Where(up => up.UserId == id)
                .Include(up => up.Project)
                .Select(up => new ProjectDto
                {
                    Id = up.Project.Id,
                    Name = up.Project.Name
                })
                .ToListAsync();

            var result = new UserDetailsDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role,
                Projects = projects
            };

            return Ok(result);
        }
    }
}

