using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Checklist.Models;
using Checklist.Models.Dtos;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Checklist.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ChecklistController : ControllerBase
    {
        private readonly DataContext _context;

        public ChecklistController(DataContext context)
        {
            _context = context;
        }

        // ? Create a new checklist and submit answers
        [HttpPost("create")]
        public async Task<IActionResult> CreateChecklist([FromBody] ChecklistCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var checklistEntity = new Checklist.Models.Checklist
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                TemplateId = dto.TemplateId,
                UserId = dto.UserId,
                Answers = dto.Answers.Select(a => new Answer
                {
                    Id = Guid.NewGuid(),
                    QuestionId = a.QuestionId,
                    AnswerValue = a.Response
                }).ToList()
            };

            _context.Checklists.Add(checklistEntity);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Checklist created successfully", checklistEntity.Id });
        }

        // ? Get all checklists (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpGet("getAll")]
        public async Task<IActionResult> GetAllChecklists()
        {
            var checklists = await _context.Checklists
                .Include(c => c.Template)
                .Include(c => c.User)
                .Include(c => c.Answers)
                .ThenInclude(a => a.Question)
                .Select(c => new ChecklistDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    TemplateName = c.Template != null ? c.Template.Name : string.Empty,
                    UserName = c.User != null ? c.User.Username : string.Empty,
                    Answers = c.Answers.Select(a => new AnswerDto
                    {
                        QuestionId = a.QuestionId,
                        Response = a.AnswerValue
                    }).ToList()
                })
                .ToListAsync();

            return Ok(checklists);
        }

        // ? Get checklist by ID
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetChecklistById(Guid id)
        {
            var checklist = await _context.Checklists
                .Include(c => c.Template)
                .Include(c => c.User)
                .Include(c => c.Answers)
                .ThenInclude(a => a.Question)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (checklist == null)
                return NotFound(new { message = "Checklist not found" });

            var dto = new ChecklistDto
            {
                Id = checklist.Id,
                Title = checklist.Title,
                TemplateName = checklist.Template != null ? checklist.Template.Name : string.Empty,
                UserName = checklist.User != null ? checklist.User.Username : string.Empty,
                Answers = checklist.Answers.Select(a => new AnswerDto
                {
                    QuestionId = a.QuestionId,
                    Response = a.AnswerValue
                }).ToList()
            };

            return Ok(dto);
        }
    }
}
