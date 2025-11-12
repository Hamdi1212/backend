using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Checklist.Models;
using Checklist.Models.Dtos;

namespace Checklist.Controllers
{
    [Authorize(Roles = "Admin")] //  Only admins can manage templates
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
        public async Task<IActionResult> DeleteTemplate(Guid id)
        {
            var template = await _context.Templates.FindAsync(id);
            if (template == null)
                return NotFound(new { message = "Template not found" });

            _context.Templates.Remove(template);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Template deleted successfully" });
        }
    }
}
