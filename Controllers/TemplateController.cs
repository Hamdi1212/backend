using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Checklist.Models;
using Checklist.Models.Dtos;

namespace Checklist.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TemplateController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ILogger<TemplateController> _logger;

        public TemplateController(DataContext context, ILogger<TemplateController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Get all templates
        [HttpGet("getAll")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetAllTemplates()
        {
            var templates = await _context.Templates
                .AsNoTracking()
                .Select(t => new TemplateDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description
                })
                .ToListAsync();

            return Ok(templates);
        }

        // Get template by ID
        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetTemplateById(Guid id)
        {
            var template = await _context.Templates
                .AsNoTracking()
                .Where(t => t.Id == id)
                .Select(t => new TemplateDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description
                })
                .FirstOrDefaultAsync();

            if (template == null)
                return NotFound(new { message = "Template not found" });

            return Ok(template);
        }

        // Create new template
        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTemplate([FromBody] TemplateCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check for duplicate template name (case-insensitive)
            if (await _context.Templates.AnyAsync(t => t.Name.ToLower() == dto.Name.ToLower()))
                return BadRequest(new { message = "A template with this name already exists" });

            var template = new Template
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description
            };

            _context.Templates.Add(template);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Template created successfully", template.Id });
        }

        // Update template
        [HttpPut("update/{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTemplate(Guid id, [FromBody] TemplateUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var template = await _context.Templates.FindAsync(id);
            if (template == null)
                return NotFound(new { message = "Template not found" });

            // Check for duplicate name (excluding current template)
            if (await _context.Templates.AnyAsync(t => t.Id != id && t.Name.ToLower() == dto.Name.ToLower()))
                return BadRequest(new { message = "A template with this name already exists" });

            template.Name = dto.Name;
            template.Description = dto.Description;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Template updated successfully" });
        }

        // Delete template
        [HttpDelete("delete/{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTemplate(Guid id)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var template = await _context.Templates
                    .Include(t => t.questions)
                    .Include(t => t.Checklists)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (template == null)
                    return NotFound(new { message = "Template not found" });

                // Check if any checklists reference this template
                if (template.Checklists is { Count: > 0 })
                {
                    return BadRequest(new
                    {
                        message = $"Cannot delete template. It is being used by {template.Checklists.Count} checklist(s).",
                        checklistCount = template.Checklists.Count
                    });
                }

                // Delete associated questions if any
                if (template.questions is { Count: > 0 })
                {
                    _logger.LogInformation("Deleting {Count} questions associated with template {TemplateId}",
                        template.questions.Count, id);
                    _context.Questions.RemoveRange(template.questions);
                }

                // Delete template
                _context.Templates.Remove(template);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Template deleted successfully" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting template {TemplateId}", id);
                return StatusCode(500, new { message = "Error deleting template" });
            }
        }

        // Get available templates (for users)
        [HttpGet("available")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetAvailableTemplates()
        {
            try
            {
                var templates = await _context.Templates
                    .AsNoTracking()
                    .Select(t => new
                    {
                        id = t.Id,
                        name = t.Name,
                        description = t.Description
                    })
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} available templates", templates.Count);

                return Ok(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching available templates");
                return StatusCode(500, new { message = "Error fetching templates" });
            }
        }
    }
}
