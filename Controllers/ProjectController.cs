using Checklist.Models;
using Checklist.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Checklist.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ILogger<ProjectController> _logger;

        public ProjectController(DataContext context, ILogger<ProjectController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // 1. Get all projects
        [HttpGet("getAll")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllProjects()
        {
            var projects = await _context.Projects
                .AsNoTracking()
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

        // 2. Get project by ID
        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetProjectById(Guid id)
        {
            var project = await _context.Projects
                .AsNoTracking()
                .Where(p => p.Id == id)
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
                .FirstOrDefaultAsync();

            if (project == null)
                return NotFound(new { message = "Project not found" });

            return Ok(project);
        }

        // 3. Create new project
        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProject([FromBody] ProjectCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check for duplicate project name (case-insensitive)
            if (await _context.Projects.AnyAsync(p => p.Name.ToLower() == dto.Name.ToLower()))
                return BadRequest(new { message = "Project with this name already exists" });

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

        // 4. Update existing project
        [HttpPut("update/{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProject(Guid id, [FromBody] ProjectUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound(new { message = "Project not found" });

            // Check for duplicate name (excluding current project)
            if (await _context.Projects.AnyAsync(p => p.Id != id && p.Name.ToLower() == dto.Name.ToLower()))
                return BadRequest(new { message = "Project with this name already exists" });

            project.Name = dto.Name;
            project.Description = dto.Description;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Project updated successfully" });
        }

        // 5. Delete project
        [HttpDelete("delete/{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProject(Guid id)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var project = await _context.Projects
                    .Include(p => p.Lines)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (project == null)
                    return NotFound(new { message = "Project not found" });

                // Delete associated lines
                if (project.Lines is { Count: > 0 })
                    _context.Lines.RemoveRange(project.Lines);

                // Delete project
                _context.Projects.Remove(project);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Project deleted successfully" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting project {ProjectId}", id);
                return StatusCode(500, new { message = "Error deleting project" });
            }
        }

        // 6. Get projects assigned to current user
        [HttpGet("my")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetMyProjects()
        {
            try
            {
                // Extract username from token
                var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst(ClaimTypes.Name)?.Value
                            ?? User.FindFirst("sub")?.Value
                            ?? User.Identity?.Name;

                if (string.IsNullOrWhiteSpace(username))
                {
                    _logger.LogWarning("Could not determine user from token");
                    return Unauthorized(new { message = "Could not determine user from token" });
                }

                // Get projects in a single optimized query
                var projects = await _context.UserProjects
                    .AsNoTracking()
                    .Where(up => up.User.Username == username)
                    .Select(up => new
                    {
                        id = up.Project.Id,
                        name = up.Project.Name,
                        description = up.Project.Description
                    })
                    .ToListAsync();

                _logger.LogInformation("Found {Count} projects for user '{Username}'", projects.Count, username);

                return Ok(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching projects for user");
                return StatusCode(500, new { message = "Error fetching projects" });
            }
        }
    }
}
