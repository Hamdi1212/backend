using Checklist.Models;
using Checklist.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Checklist.Controllers
{
    [Authorize] // Allow authenticated users, restrict specific methods as needed
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController : ControllerBase
    {
        private readonly DataContext _context;

        public ProjectController(DataContext context)
        {
            _context = context;
        }

        //  1. Get all projects
        [HttpGet("getAll")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllProjects()
        {
            var projects = await _context.Projects
                .Include(p => p.Lines)
                .Select(p => new ProjectDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Lines = p.Lines != null
                        ? p.Lines.Select(l => new LineDto
                        {
                            Id = l.Id,
                            Name = l.Name,
                            ProjectId = l.ProjectId
                        }).ToList()
                        : new List<LineDto>()
                })
                .ToListAsync();

            return Ok(projects);
        }

        //  2. Get project by ID
        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetProjectById(Guid id)
        {
            var project = await _context.Projects
                .Include(p => p.Lines)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                return NotFound(new { message = "Project not found" });

            var dto = new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Lines = project.Lines?.Select(l => new LineDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    ProjectId = l.ProjectId
                }).ToList()
            };

            return Ok(dto);
        }

        // 3. Create new project
        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProject([FromBody] ProjectCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Project created successfully", project.Id });
        }

        //  4. Update existing project
        [HttpPut("update/{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProject(Guid id, [FromBody] ProjectUpdateDto dto)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound(new { message = "Project not found" });

            project.Name = dto.Name;
            project.Description = dto.Description;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Project updated successfully" });
        }

        //  5. Delete project
        [HttpDelete("delete/{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProject(Guid id)
        {
            var project = await _context.Projects
                .Include(p => p.Lines)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                return NotFound(new { message = "Project not found" });

            // Optional: delete lines first to avoid foreign key issues
            if (project.Lines != null && project.Lines.Any())
                _context.Lines.RemoveRange(project.Lines);

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Project deleted successfully" });
        }
        [HttpGet("my")]
        [Authorize(Roles = "Admin,User")] // Allow both Admin and User roles
        public async Task<IActionResult> GetMyProjects()
        {
            try
            {
                // Extract username from token (same logic as ChecklistController /my)
                var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst(ClaimTypes.Name)?.Value
                            ?? User.FindFirst("sub")?.Value
                            ?? User.Identity?.Name;

                if (string.IsNullOrWhiteSpace(username))
                {
                    return Unauthorized(new { message = "Could not determine user from token" });
                }

                Console.WriteLine($"[GetMyProjects] Username from token: '{username}'");

                // Find user in database
                var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (dbUser == null)
                {
                    Console.WriteLine($"[GetMyProjects] User '{username}' not found in database");
                    return NotFound(new { message = "User not found" });
                }

                Console.WriteLine($"[GetMyProjects] User found: {dbUser.Id}");

                // Get projects assigned to this user through UserProjects table
                var projects = await _context.UserProjects
                    .Where(up => up.UserId == dbUser.Id)
                    .Include(up => up.Project)
                    .Select(up => new
                    {
                        id = up.Project.Id,
                        name = up.Project.Name,
                        description = up.Project.Description
                    })
                    .ToListAsync();

                Console.WriteLine($"[GetMyProjects] Found {projects.Count} projects for user '{username}'");

                return Ok(projects);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetMyProjects] Error: {ex.Message}");
                return StatusCode(500, new { message = "Error fetching projects", error = ex.Message });
            }
        }
    }
}
