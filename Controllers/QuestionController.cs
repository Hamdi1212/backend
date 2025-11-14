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
    public class QuestionController : ControllerBase
    {
        private readonly DataContext _context;

        public QuestionController(DataContext context)
        {
            _context = context;
        }

        //  1. Get all questions
        [HttpGet("getAll")]
        public async Task<IActionResult> GetAllQuestions()
        {
            var questions = await _context.Questions
                .Include(q => q.Template)
                .Select(q => new QuestionDto
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                   
                    TemplateId = q.TemplateId,
                    TemplateName = q.Template.Name
                })
                .ToListAsync();

            return Ok(questions);
        }

        //  2. Get questions by template
        [HttpGet("getByTemplate/{templateId:guid}")]
        public async Task<IActionResult> GetByTemplate(Guid templateId)
        {
            var questions = await _context.Questions
                .Where(q => q.TemplateId == templateId)
                .Select(q => new QuestionDto
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    
                    TemplateId = q.TemplateId
                })
                .ToListAsync();

            return Ok(questions);
        }

        //  3. Create question
        [HttpPost("create")]
        public async Task<IActionResult> CreateQuestion([FromBody] QuestionCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var template = await _context.Templates.FindAsync(dto.TemplateId);
            if (template == null)
                return NotFound(new { message = "Template not found" });

            var question = new Question
            {
                Id = Guid.NewGuid(),
                QuestionText = dto.QuestionText,
                
                TemplateId = dto.TemplateId
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Question created successfully", question.Id });
        }

        //  4. Update question
        [HttpPut("update/{id:guid}")]
        public async Task<IActionResult> UpdateQuestion(Guid id, [FromBody] QuestionUpdateDto dto)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question == null)
                return NotFound(new { message = "Question not found" });

            question.QuestionText = dto.QuestionText;
            question.TemplateId = dto.TemplateId;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Question updated successfully" });
        }

        //  5. Delete question
        [HttpDelete("delete/{id:guid}")]
        public async Task<IActionResult> DeleteQuestion(Guid id)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question == null)
                return NotFound(new { message = "Question not found" });

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Question deleted successfully" });
        }
    }
}
