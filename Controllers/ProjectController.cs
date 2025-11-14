using Checklist.Models;
using Checklist.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Checklist.Controllers
{
    [Authorize(Roles = "Admin")]
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
        public async Task<IActionResult> GetAllProjects()
        {
            var projects = await _context.Projects
                .Include(p => p.Lines)
                .Select(p => new ProjectDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Lines = p.Lines.Select(l => new LineDto
                    {
                        Id = l.Id,
                        Name = l.Name,
                        ProjectId = l.ProjectId
                    }).ToList()
                })
                .ToListAsync();

            return Ok(projects);
        }

        //  2. Get project by ID
        [HttpGet("{id:guid}")]
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
    }
}
