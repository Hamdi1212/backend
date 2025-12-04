using Checklist.Models;
using Checklist.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Checklist.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ChecklistController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ILogger<ChecklistController> _logger;

        public ChecklistController(DataContext context, ILogger<ChecklistController> logger)
        {
            _context = context;
            _logger = logger;
        }

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

            // Validate template and line exist
            var templateExists = await _context.Templates.AnyAsync(t => t.Id == dto.TemplateId);
            var lineExists = await _context.Lines.AnyAsync(l => l.Id == dto.LineId);
            if (!templateExists || !lineExists)
                return BadRequest(new { message = "TemplateId ou LineId invalide." });

            // Validate project exists
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
                Date = dto.Date ?? DateTime.UtcNow,
                Shift = dto.Shift,
                NotificationStatus = "Pending"
            };

            _context.Checklists.Add(checklist);
            await _context.SaveChangesAsync();

            return Ok(new { checklistId = checklist.Id, message = "Checklist démarrée." });
        }

        // BACKEND FIX - Replace the submit method in your ChecklistController.cs

        [Authorize(Roles = "User")]
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitChecklist([FromBody] ChecklistSubmitDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var checklist = await _context.Checklists
                    .Include(c => c.Answers)
                    .FirstOrDefaultAsync(c => c.Id == dto.ChecklistId);

                if (checklist == null)
                    return NotFound(new { message = "Checklist introuvable." });

                // Prevent double submission
                if (string.Equals(checklist.status, "Completed", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { message = "Checklist déjà soumise." });

                // Get username from token
                var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                            ?? User.Identity?.Name;

                if (string.IsNullOrEmpty(username))
                    return Unauthorized(new { message = "Could not determine user from token" });

                // Save answers
                var answerEntities = dto.Answers.Select(a => new Answer
                {
                    Id = Guid.NewGuid(),
                    ChecklistId = checklist.Id,
                    QuestionId = a.QuestionId,
                    AnswerValue = a.AnswerValue
                }).ToList();

                _context.Answers.AddRange(answerEntities);

                // Check for NOK answers
                var nokAnswers = answerEntities
                    .Where(a => a.AnswerValue.Equals("NOK", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // Validate: If there are NOK answers, action plans must be provided
                if (nokAnswers.Any())
                {
                    if (dto.ActionPlans == null || !dto.ActionPlans.Any())
                    {
                        return BadRequest(new
                        {
                            message = $"You have {nokAnswers.Count} NOK answer(s). Action plans are required for all NOK answers."
                        });
                    }

                    // FIXED: Validate by QuestionId instead of AnswerId
                    var nokQuestionIds = nokAnswers.Select(a => a.QuestionId).ToHashSet();
                    var actionPlanQuestionIds = dto.ActionPlans.Select(ap => ap.QuestionId).ToHashSet();

                    if (!nokQuestionIds.SetEquals(actionPlanQuestionIds))
                    {
                        return BadRequest(new
                        {
                            message = "All NOK answers must have corresponding action plans."
                        });
                    }

                    // Get questions with their order for numbering
                    var questionIds = nokAnswers.Select(a => a.QuestionId).ToList();
                    var allQuestionsInTemplate = await _context.Questions
                        .Where(q => q.TemplateId == checklist.TemplateId)
                        .OrderBy(q => q.Id)
                        .Select(q => new { q.Id, q.QuestionText })
                        .ToListAsync();

                    // Create action plans
                    foreach (var actionPlanDto in dto.ActionPlans)
                    {
                        // FIXED: Find the answer by QuestionId instead of AnswerId
                        var correspondingAnswer = answerEntities
                            .FirstOrDefault(a => a.QuestionId == actionPlanDto.QuestionId);

                        if (correspondingAnswer == null)
                        {
                            _logger.LogWarning("Answer for question {QuestionId} not found",
                                actionPlanDto.QuestionId);
                            continue;
                        }

                        // Find question number (1-based index)
                        var questionIndex = allQuestionsInTemplate
                            .FindIndex(q => q.Id == actionPlanDto.QuestionId);

                        if (questionIndex == -1)
                        {
                            _logger.LogWarning("Question {QuestionId} not found for action plan",
                                actionPlanDto.QuestionId);
                            continue;
                        }

                        var questionNumber = questionIndex + 1;

                        var actionPlan = new ActionPlan
                        {
                            Id = Guid.NewGuid(),
                            ChecklistId = checklist.Id,
                            AnswerId = correspondingAnswer.Id, // Use the answer ID we just created
                            QuestionId = actionPlanDto.QuestionId,
                            NokPointNumber = questionNumber,
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = username,
                            Actions = actionPlanDto.Actions,
                            Responsables = actionPlanDto.Responsables,
                            DateCloture = actionPlanDto.DateCloture,
                            Status = "Open"
                        };

                        _context.ActionPlans.Add(actionPlan);
                    }
                }

                // Update checklist with matricules
                checklist.QualityOperatorMatricule = dto.QualityOperatorMatricule;
                checklist.ProductionOperatorMatricule = dto.ProductionOperatorMatricule;
                checklist.status = "Completed";
                checklist.Date = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Checklist {ChecklistId} submitted successfully with {ActionPlanCount} action plans",
                    checklist.Id, dto.ActionPlans?.Count ?? 0);

                return Ok(new
                {
                    message = "Checklist submitted successfully",
                    checklistId = checklist.Id,
                    nokCount = nokAnswers.Count,
                    actionPlansCreated = dto.ActionPlans?.Count ?? 0
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error submitting checklist {ChecklistId}", dto.ChecklistId);
                return StatusCode(500, new { message = "Error submitting checklist", error = ex.Message });
            }
        }


        [Authorize(Roles = "User")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyChecklists()
        {
            try
            {
                var username = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("sub")?.Value;

                _logger.LogInformation("[GetMyChecklists] Username from token: '{Username}'", username);

                if (string.IsNullOrEmpty(username))
                    return Unauthorized(new { message = "Invalid token - no username claim" });

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                _logger.LogInformation("[GetMyChecklists] User found: {UserFound}, UserId: {UserId}",
                    user != null, user?.Id);

                if (user == null)
                    return NotFound(new { message = $"User not found: {username}" });

                var checklists = await _context.Checklists
                    .AsNoTracking()
                    .Where(c => c.UserId == user.Id)
                    .Include(c => c.Template)
                    .Include(c => c.Project)
                    .Include(c => c.Line)
                    .Select(c => new
                    {
                        c.Id,
                        TemplateName = c.Template != null ? c.Template.Name : "Unknown",
                        ProjectName = c.Project != null ? c.Project.Name : "",
                        LineName = c.Line != null ? c.Line.Name : "",
                        Status = c.status ?? "Pending",
                        c.Date,
                        c.Shift
                    })
                    .ToListAsync();

                _logger.LogInformation("[GetMyChecklists] Found {Count} checklists for user '{Username}'",
                    checklists.Count, username);

                return Ok(checklists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetMyChecklists] Error fetching checklists");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetChecklistById(Guid id)
        {
            var checklist = await _context.Checklists
                .AsNoTracking()
                .Include(c => c.Template)
                .Include(c => c.User)
                .Include(c => c.Project)
                .Include(c => c.Line)
                .Include(c => c.Answers)
                    .ThenInclude(a => a.Question)
                .Include(c => c.ActionPlans)
                    .ThenInclude(ap => ap.Question)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (checklist == null)
                return NotFound(new { message = "Checklist not found" });

            var response = new
            {
                id = checklist.Id,
                templateName = checklist.Template?.Name,
                userName = checklist.User?.Username,
                projectName = checklist.Project?.Name,
                lineName = checklist.Line?.Name,
                status = checklist.status,
                date = checklist.Date,
                shift = checklist.Shift,
                qualityOperatorMatricule = checklist.QualityOperatorMatricule,
                productionOperatorMatricule = checklist.ProductionOperatorMatricule,
                answers = checklist.Answers?.Select(a => new
                {
                    id = a.Id,
                    questionId = a.QuestionId,
                    questionText = a.Question?.QuestionText,
                    answerValue = a.AnswerValue
                }),
                actionPlans = checklist.ActionPlans?.Select(ap => new
                {
                    id = ap.Id,
                    questionId = ap.QuestionId,
                    questionText = ap.Question?.QuestionText,
                    nokPointNumber = ap.NokPointNumber,
                    actions = ap.Actions,
                    responsables = ap.Responsables,
                    dateCloture = ap.DateCloture,
                    status = ap.Status,
                    createdBy = ap.CreatedBy,
                    createdDate = ap.CreatedDate
                })
            };

            return Ok(response);
        }
    }
}
