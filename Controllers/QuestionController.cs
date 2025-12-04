using Checklist.Models;
using Checklist.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Checklist.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ILogger<QuestionController> _logger;

        public QuestionController(DataContext context, ILogger<QuestionController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // 1. Get all questions
        [HttpGet("getAll")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllQuestions()
        {
            var questions = await _context.Questions
                .AsNoTracking()
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

        // 2. Get questions by template
        [HttpGet("getByTemplate/{templateId:guid}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetByTemplate(Guid templateId)
        {
            // Validate template exists
            if (!await _context.Templates.AnyAsync(t => t.Id == templateId))
                return NotFound(new { message = "Template not found" });

            var questions = await _context.Questions
                .AsNoTracking()
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

        // 3. Create question
        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateQuestion([FromBody] QuestionCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate template exists
            if (!await _context.Templates.AnyAsync(t => t.Id == dto.TemplateId))
                return NotFound(new { message = "Template not found" });

            // Check for duplicate question in same template (case-insensitive)
            if (await _context.Questions.AnyAsync(q => 
                q.TemplateId == dto.TemplateId && 
                q.QuestionText.ToLower() == dto.QuestionText.ToLower()))
            {
                return BadRequest(new { message = "A question with this text already exists in this template" });
            }

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

        // 4. Update question
        [HttpPut("update/{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateQuestion(Guid id, [FromBody] QuestionUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var question = await _context.Questions.FindAsync(id);
            if (question == null)
                return NotFound(new { message = "Question not found" });

            // Validate new template exists if changing template
            if (question.TemplateId != dto.TemplateId)
            {
                if (!await _context.Templates.AnyAsync(t => t.Id == dto.TemplateId))
                    return NotFound(new { message = "Template not found" });
            }

            // Check for duplicate question text in target template (excluding current question)
            if (await _context.Questions.AnyAsync(q => 
                q.Id != id && 
                q.TemplateId == dto.TemplateId && 
                q.QuestionText.ToLower() == dto.QuestionText.ToLower()))
            {
                return BadRequest(new { message = "A question with this text already exists in this template" });
            }

            question.QuestionText = dto.QuestionText;
            question.TemplateId = dto.TemplateId;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Question updated successfully" });
        }

        // 5. Delete question
        [HttpDelete("delete/{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteQuestion(Guid id)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var question = await _context.Questions
                    .Include(q => q.Answers)
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (question == null)
                    return NotFound(new { message = "Question not found" });

                // Delete associated answers if any
                if (question.Answers is { Count: > 0 })
                {
                    _logger.LogInformation("Deleting {Count} answers associated with question {QuestionId}", 
                        question.Answers.Count, id);
                    _context.Answers.RemoveRange(question.Answers);
                }

                // Delete question
                _context.Questions.Remove(question);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Question deleted successfully" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting question {QuestionId}", id);
                return StatusCode(500, new { message = "Error deleting question" });
            }
        }
    }
}
