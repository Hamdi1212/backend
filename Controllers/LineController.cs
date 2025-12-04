using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Checklist.Models;
using Checklist.Models.Dtos;

namespace Checklist.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LineController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ILogger<LineController> _logger;

        public LineController(DataContext context, ILogger<LineController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET all lines
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllLines()
        {
            var lines = await _context.Lines
                .AsNoTracking()
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

        // GET lines by Project ID
        [HttpGet("byProject/{projectId:guid}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetLinesByProject(Guid projectId)
        {
            // Validate project exists
            if (!await _context.Projects.AnyAsync(p => p.Id == projectId))
                return NotFound(new { message = "Project not found" });

            var lines = await _context.Lines
                .AsNoTracking()
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

        // GET line by ID
        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetLineById(Guid id)
        {
            var line = await _context.Lines
                .AsNoTracking()
                .Where(l => l.Id == id)
                .Select(l => new LineDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    ProjectId = l.ProjectId,
                    ProjectName = l.Project != null ? l.Project.Name : null
                })
                .FirstOrDefaultAsync();

            if (line == null)
                return NotFound(new { message = "Line not found" });

            return Ok(line);
        }

        // POST create new line
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateLine([FromBody] LineCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate project exists
            var project = await _context.Projects.FindAsync(dto.ProjectId);
            if (project == null)
                return NotFound(new { message = "Project not found" });

            // Check for duplicate line name in same project (case-insensitive)
            if (await _context.Lines.AnyAsync(l => 
                l.ProjectId == dto.ProjectId && 
                l.Name.ToLower() == dto.Name.ToLower()))
            {
                return BadRequest(new { message = "A line with this name already exists in this project" });
            }

            var line = new Line
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                ProjectId = dto.ProjectId
            };

            _context.Lines.Add(line);
            await _context.SaveChangesAsync();

            var result = new LineDto
            {
                Id = line.Id,
                Name = line.Name,
                ProjectId = line.ProjectId,
                ProjectName = project.Name
            };

            return CreatedAtAction(nameof(GetLineById), new { id = line.Id }, result);
        }

        // PUT update existing line
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateLine(Guid id, [FromBody] LineUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var line = await _context.Lines.FindAsync(id);
            if (line == null)
                return NotFound(new { message = "Line not found" });

            // Validate new project exists if changing project
            if (line.ProjectId != dto.ProjectId)
            {
                if (!await _context.Projects.AnyAsync(p => p.Id == dto.ProjectId))
                    return NotFound(new { message = "Project not found" });
            }

            // Check for duplicate line name in target project (excluding current line)
            if (await _context.Lines.AnyAsync(l => 
                l.Id != id && 
                l.ProjectId == dto.ProjectId && 
                l.Name.ToLower() == dto.Name.ToLower()))
            {
                return BadRequest(new { message = "A line with this name already exists in this project" });
            }

            line.Name = dto.Name;
            line.ProjectId = dto.ProjectId;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Line updated successfully" });
        }

        // DELETE line
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteLine(Guid id)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var line = await _context.Lines
                    .Include(l => l.Checklists)
                    .FirstOrDefaultAsync(l => l.Id == id);

                if (line == null)
                    return NotFound(new { message = "Line not found" });

                // Delete associated checklists if any
                if (line.Checklists is { Count: > 0 })
                {
                    _logger.LogInformation("Deleting {Count} checklists associated with line {LineId}", 
                        line.Checklists.Count, id);
                    _context.Checklists.RemoveRange(line.Checklists);
                }

                // Delete line
                _context.Lines.Remove(line);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Line deleted successfully" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting line {LineId}", id);
                return StatusCode(500, new { message = "Error deleting line" });
            }
        }
    }
}
