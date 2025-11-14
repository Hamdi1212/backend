using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Checklist.Models;
using Checklist.Models.Dtos;

namespace Checklist.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class LineController : ControllerBase
    {
        private readonly DataContext _context;

        public LineController(DataContext context)
        {
            _context = context;
        }

        //  GET all lines
        [HttpGet]
        public async Task<IActionResult> GetAllLines()
        {
            var lines = await _context.Lines
                .Include(l => l.Project)
                .Select(l => new LineDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    ProjectId = l.ProjectId,
                    ProjectName = l.Project != null ? l.Project.Name : null
                })
                .ToListAsync();

            return Ok(lines);
        }

        //  GET lines by Project ID
        [HttpGet("byProject/{projectId:guid}")]
        public async Task<IActionResult> GetLinesByProject(Guid projectId)
        {
            var lines = await _context.Lines
                .Where(l => l.ProjectId == projectId)
                .Select(l => new LineDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    ProjectId = l.ProjectId
                })
                .ToListAsync();

            return Ok(lines);
        }

        //  GET line by ID
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetLineById(Guid id)
        {
            var line = await _context.Lines
                .Include(l => l.Project)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (line == null)
                return NotFound(new { message = "Line not found" });

            var result = new LineDto
            {
                Id = line.Id,
                Name = line.Name,
                ProjectId = line.ProjectId,
                ProjectName = line.Project != null ? line.Project.Name : null
            };

            return Ok(result);
        }

        //  POST create new line
        [HttpPost]
        public async Task<IActionResult> CreateLine([FromBody] LineCreateDto dto)
        {
            var project = await _context.Projects.FindAsync(dto.ProjectId);
            if (project == null)
                return NotFound(new { message = "Project not found" });

            var line = new Line
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                ProjectId = dto.ProjectId
            };

            _context.Lines.Add(line);
            await _context.SaveChangesAsync();

            // Retourner un DTO plat pour éviter la boucle de sérialisation
            var result = new LineDto
            {
                Id = line.Id,
                Name = line.Name,
                ProjectId = line.ProjectId,
                ProjectName = project.Name
            };

            return CreatedAtAction(nameof(GetLineById), new { id = line.Id }, result);
        }

        //  PUT update existing line
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateLine(Guid id, [FromBody] LineUpdateDto dto)
        {
            var line = await _context.Lines.FindAsync(id);
            if (line == null)
                return NotFound(new { message = "Line not found" });

            var project = await _context.Projects.FindAsync(dto.ProjectId);
            if (project == null)
                return NotFound(new { message = "Project not found" });

            line.Name = dto.Name;
            line.ProjectId = dto.ProjectId;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Line updated successfully" });
        }

        //  DELETE line
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteLine(Guid id)
        {
            var line = await _context.Lines.FindAsync(id);
            if (line == null)
                return NotFound(new { message = "Line not found" });

            _context.Lines.Remove(line);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Line deleted successfully" });
        }
    }
}
