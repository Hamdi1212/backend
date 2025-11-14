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

        // ? Create a new checklist and submit answers (existing)
        [HttpPost("create")]
        public async Task<IActionResult> CreateChecklist([FromBody] ChecklistCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var checklistEntity = new Checklist.Models.Checklist
            {
                Id = Guid.NewGuid(),
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
        [Authorize(Roles = "User")]
        [HttpPost("start")]
        public async Task<IActionResult> StartChecklist([FromBody] ChecklistStartDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Vérifier que le template et la line existent
            var templateExists = await _context.Templates.AnyAsync(t => t.Id == dto.TemplateId);
            var lineExists = await _context.Lines.AnyAsync(l => l.Id == dto.LineId);
            if (!templateExists || !lineExists)
                return BadRequest(new { message = "TemplateId ou LineId invalide." });

            // Vérifier que le project existe
            var projectExists = await _context.Projects.AnyAsync(p => p.Id == dto.ProjectId);
            if (!projectExists)
                return BadRequest(new { message = "ProjectId invalide." });

            var checklist = new Checklist.Models.Checklist
            {
                Id = Guid.NewGuid(),
                UserId = dto.UserId,
                ProjectId = dto.ProjectId,
                LineId = dto.LineId,
                TemplateId = dto.TemplateId,
                status = "Pending",
                // set new fields from DTO
                Date = dto.Date ?? DateTime.UtcNow,
                Shift = dto.Shift,
                NotificationStatus = "Pending"
            };

            _context.Checklists.Add(checklist);
            await _context.SaveChangesAsync();

            return Ok(new { checklistId = checklist.Id, message = "Checklist démarrée." });
        }

        // ? Submit checklist (User) — enregistre réponses, empêche double-soumission

        // ? Submit checklist (User) — enregistre réponses, empêche double-soumission
        [Authorize(Roles = "User")]
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitChecklist([FromBody] ChecklistSubmitDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var checklist = await _context.Checklists.FindAsync(dto.ChecklistId);
            if (checklist == null)
                return NotFound(new { message = "Checklist introuvable." });

            // Empêcher double-soumission
            if (string.Equals(checklist.status, "Completed", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "Checklist déjà soumise." });

            // Map des réponses et insertion
            var answers = dto.Answers.Select(a => new Answer
            {
                Id = Guid.NewGuid(),
                ChecklistId = checklist.Id,
                QuestionId = a.QuestionId,
                AnswerValue = a.AnswerValue
            }).ToList();

            _context.Answers.AddRange(answers);

            // Mettre à jour le statut
            checklist.status = "Completed";
            // mark submission date
            checklist.Date = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Ici on peut déclencher génération PDF / envoi e-mail (prochaine étape : background tasks)
            return Ok(new { message = "Réponses sauvegardées et checklist soumise." });
        }
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
