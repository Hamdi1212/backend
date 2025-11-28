using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Checklist.Models;
using Checklist.Models.Dtos;

namespace Checklist.Controllers
{
    [Authorize] // Allow authenticated users, restrict specific methods as needed
    [ApiController]
    [Route("api/[controller]")]
    public class TemplateController : ControllerBase
    {
        private readonly DataContext _context;

        public TemplateController(DataContext context)
        {
            _context = context;
        }

        //  Get all templates
        [HttpGet("getAll")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllTemplates()
        {
            var templates = await _context.Templates
                .Select(t => new TemplateDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description
                })
                .ToListAsync();

            return Ok(templates);
        }

        //  Get template by ID
        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin,User")] // Allow both Admin and User roles
        public async Task<IActionResult> GetTemplateById(Guid id)
        {
            var template = await _context.Templates.FindAsync(id);
            if (template == null)
                return NotFound(new { message = "Template not found" });

            var dto = new TemplateDto
            {
                Id = template.Id,
                Name = template.Name,
                Description = template.Description
            };

            return Ok(dto);
        }

        //  Create new template
        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTemplate([FromBody] TemplateCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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

        //  Update template
        [HttpPut("update/{id:guid}")]
        [Authorize(Roles = "Admin")] // Only Admins can update templates
        public async Task<IActionResult> UpdateTemplate(Guid id, [FromBody] TemplateUpdateDto dto)
        {
            var template = await _context.Templates.FindAsync(id);
            if (template == null)
                return NotFound(new { message = "Template not found" });

            template.Name = dto.Name;
            template.Description = dto.Description;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Template updated successfully" });
        }

        //  Delete template
        [HttpDelete("delete/{id:guid}")]
        [Authorize(Roles = "Admin")] // Only Admins can delete templates
        public async Task<IActionResult> DeleteTemplate(Guid id)
        {
            var template = await _context.Templates.FindAsync(id);
            if (template == null)
                return NotFound(new { message = "Template not found" });

            // Check if any checklists reference this template
            var checklistCount = await _context.Checklists
                .Where(c => c.TemplateId == id)
                .CountAsync();
            
            if (checklistCount > 0)
            {
                return BadRequest(new { 
                    message = $"Cannot delete template. It is being used by {checklistCount} checklist(s).",
                    checklistCount = checklistCount
                });
            }

            _context.Templates.Remove(template);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Template deleted successfully" });
        }
        [HttpGet("available")]
        [Authorize(Roles = "Admin,User")] // Allow both Admin and User roles
        public async Task<IActionResult> GetAvailableTemplates()
        {
            try
            {
                var templates = await _context.Templates
                    .Select(t => new
                    {
                        id = t.Id,
                        name = t.Name,
                        description = t.Description
                    })
                    .ToListAsync();

                return Ok(templates);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetAvailableTemplates] Error: {ex.Message}");
                return StatusCode(500, new { message = "Error fetching templates", error = ex.Message });
            }
        }
    }
}
