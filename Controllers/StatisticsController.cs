using Checklist.Models;
using Checklist.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Checklist.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ILogger<StatisticsController> _logger;

        public StatisticsController(DataContext context, ILogger<StatisticsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Statistics/dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardOverview()
        {
            try
            {
                var today = DateTime.UtcNow.Date;

                // Get projects with checklist counts first
                var projectsWithCounts = await _context.Projects
                    .Select(p => new
                    {
                        ProjectId = p.Id,
                        ProjectName = p.Name,
                        ChecklistCount = _context.Checklists.Count(c => c.ProjectId == p.Id)
                    })
                    .OrderByDescending(p => p.ChecklistCount)
                    .Take(5)
                    .ToListAsync();

                // Calculate quality scores separately for each project
                var topProjects = new List<ProjectSummaryData>();
                foreach (var project in projectsWithCounts)
                {
                    var qualityScore = CalculateProjectQualityScore(project.ProjectId); // FIXED: removed 'await' and 'Async'
                    topProjects.Add(new ProjectSummaryData
                    {
                        ProjectId = project.ProjectId,
                        ProjectName = project.ProjectName,
                        ChecklistCount = project.ChecklistCount,
                        QualityScore = qualityScore
                    });
                }

                var overview = new DashboardOverviewDto
                {
                    TotalProjects = await _context.Projects.CountAsync(),
                    TotalUsers = await _context.Users.CountAsync(),
                    TotalTemplates = await _context.Templates.CountAsync(),
                    TotalChecklists = await _context.Checklists.CountAsync(),
                    TodayChecklists = await _context.Checklists
                        .CountAsync(c => c.Date.HasValue && c.Date.Value.Date == today),
                    PendingActionPlans = await _context.ActionPlans
                        .CountAsync(ap => ap.Status == "Open"),
                    
                    // Calculate overall quality score (OK answers / Total answers * 100)
                    OverallQualityScore = await CalculateOverallQualityScore(),

                    TopProjects = topProjects,

                    RecentActivities = await _context.Checklists
                        .Include(c => c.User)
                        .Include(c => c.Project)
                        .Where(c => c.status == "Completed")
                        .OrderByDescending(c => c.Date)
                        .Take(10)
                        .Select(c => new RecentActivityData
                        {
                            Date = c.Date ?? DateTime.UtcNow,
                            Activity = "Checklist Completed",
                            UserName = c.User != null ? c.User.FullName : "Unknown",
                            ProjectName = c.Project != null ? c.Project.Name : "Unknown"
                        })
                        .ToListAsync()
                };

                return Ok(overview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard overview");
                return StatusCode(500, new { message = "Error retrieving dashboard data", error = ex.Message });
            }
        }

        // GET: api/Statistics/project/{projectId}
        [HttpGet("project/{projectId:guid}")]
        public async Task<IActionResult> GetProjectStatistics(
            Guid projectId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var project = await _context.Projects.FindAsync(projectId);
                if (project == null)
                    return NotFound(new { message = "Project not found" });

                var query = _context.Checklists
                    .Where(c => c.ProjectId == projectId);

                // Apply date filters
                if (startDate.HasValue)
                    query = query.Where(c => c.Date >= startDate.Value);
                
                if (endDate.HasValue)
                {
                    var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(c => c.Date <= endOfDay);
                }

                var checklists = await query
                    .Include(c => c.Answers)
                    .Include(c => c.ActionPlans)
                    .Include(c => c.Template)
                    .Include(c => c.Line)
                    .Include(c => c.User)
                    .ToListAsync();

                var totalChecklists = checklists.Count;
                var completedChecklists = checklists.Count(c => c.status == "Completed");
                var totalAnswers = checklists.SelectMany(c => c.Answers ?? new List<Answer>()).Count();
                var okAnswers = checklists.SelectMany(c => c.Answers ?? new List<Answer>())
                    .Count(a => a.AnswerValue == "OK");
                var nokAnswers = checklists.SelectMany(c => c.Answers ?? new List<Answer>())
                    .Count(a => a.AnswerValue == "NOK");
                var totalActionPlans = checklists.SelectMany(c => c.ActionPlans ?? new List<ActionPlan>()).Count();
                var closedActionPlans = checklists.SelectMany(c => c.ActionPlans ?? new List<ActionPlan>())
                    .Count(ap => ap.Status == "Closed");

                var statistics = new ProjectStatisticsDto
                {
                    ProjectId = projectId,
                    ProjectName = project.Name,
                    
                    TotalChecklists = totalChecklists,
                    CompletedChecklists = completedChecklists,
                    PendingChecklists = totalChecklists - completedChecklists,
                    CompletionRate = totalChecklists > 0 ? (decimal)completedChecklists / totalChecklists * 100 : 0,
                    
                    TotalAnswers = totalAnswers,
                    TotalOKAnswers = okAnswers,
                    TotalNOKAnswers = nokAnswers,
                    OKRate = totalAnswers > 0 ? (decimal)okAnswers / totalAnswers * 100 : 0,
                    NOKRate = totalAnswers > 0 ? (decimal)nokAnswers / totalAnswers * 100 : 0,
                    
                    TotalActionPlans = totalActionPlans,
                    OpenActionPlans = totalActionPlans - closedActionPlans,
                    ClosedActionPlans = closedActionPlans,
                    ActionPlanClosureRate = totalActionPlans > 0 ? (decimal)closedActionPlans / totalActionPlans * 100 : 0,

                    // Checklist trends over time
                    ChecklistTrends = checklists
                        .Where(c => c.Date.HasValue)
                        .GroupBy(c => c.Date.Value.Date)
                        .Select(g => new ChecklistTrendData
                        {
                            Date = g.Key,
                            TotalChecklists = g.Count(),
                            CompletedChecklists = g.Count(c => c.status == "Completed"),
                            NOKCount = g.SelectMany(c => c.Answers ?? new List<Answer>())
                                .Count(a => a.AnswerValue == "NOK")
                        })
                        .OrderBy(t => t.Date)
                        .ToList(),

                    // Template usage
                    TemplateUsage = checklists
                        .Where(c => c.Template != null)
                        .GroupBy(c => c.Template.Name)
                        .Select(g => new TemplateUsageData
                        {
                            TemplateName = g.Key,
                            UsageCount = g.Count(),
                            AverageNOKRate = CalculateAverageNOKRate(g.ToList())
                        })
                        .OrderByDescending(t => t.UsageCount)
                        .ToList(),

                    // Line performance
                    LinePerformance = checklists
                        .Where(c => c.Line != null)
                        .GroupBy(c => c.Line.Name)
                        .Select(g => new LinePerformanceData
                        {
                            LineName = g.Key,
                            TotalChecklists = g.Count(),
                            NOKCount = g.SelectMany(c => c.Answers ?? new List<Answer>())
                                .Count(a => a.AnswerValue == "NOK"),
                            NOKRate = CalculateGroupNOKRate(g.ToList())
                        })
                        .OrderByDescending(l => l.NOKCount)
                        .ToList(),

                    // User productivity
                    UserProductivity = checklists
                        .Where(c => c.User != null)
                        .GroupBy(c => c.User.FullName)
                        .Select(g => new UserProductivityData
                        {
                            UserName = g.Key,
                            ChecklistsCompleted = g.Count(c => c.status == "Completed"),
                            ActionPlansCreated = g.SelectMany(c => c.ActionPlans ?? new List<ActionPlan>()).Count()
                        })
                        .OrderByDescending(u => u.ChecklistsCompleted)
                        .ToList(),

                    // Shift distribution
                    ShiftDistribution = checklists
                        .Where(c => !string.IsNullOrEmpty(c.Shift))
                        .GroupBy(c => c.Shift)
                        .Select(g => new ShiftDistributionData
                        {
                            Shift = g.Key,
                            Count = g.Count(),
                            NOKRate = CalculateGroupNOKRate(g.ToList())
                        })
                        .ToList()
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting project statistics for {ProjectId}", projectId);
                return StatusCode(500, new { message = "Error retrieving statistics", error = ex.Message });
            }
        }

        // GET: api/Statistics/comparison
        [HttpGet("comparison")]
        public async Task<IActionResult> GetProjectComparison([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var projects = await _context.Projects.ToListAsync();
                var comparison = new List<object>();

                foreach (var project in projects)
                {
                    var query = _context.Checklists.Where(c => c.ProjectId == project.Id);

                    if (startDate.HasValue)
                        query = query.Where(c => c.Date >= startDate.Value);
                    
                    if (endDate.HasValue)
                    {
                        var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                        query = query.Where(c => c.Date <= endOfDay);
                    }

                    var checklists = await query.Include(c => c.Answers).ToListAsync();
                    var totalAnswers = checklists.SelectMany(c => c.Answers ?? new List<Answer>()).Count();
                    var okAnswers = checklists.SelectMany(c => c.Answers ?? new List<Answer>())
                        .Count(a => a.AnswerValue == "OK");

                    comparison.Add(new
                    {
                        projectId = project.Id,
                        projectName = project.Name,
                        totalChecklists = checklists.Count,
                        completedChecklists = checklists.Count(c => c.status == "Completed"),
                        qualityScore = totalAnswers > 0 ? (decimal)okAnswers / totalAnswers * 100 : 0,
                        avgChecklistsPerDay = CalculateAvgChecklistsPerDay(checklists, startDate, endDate)
                    });
                }

                return Ok(comparison);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting project comparison");
                return StatusCode(500, new { message = "Error retrieving comparison data", error = ex.Message });
            }
        }

        // Helper methods
        private async Task<decimal> CalculateOverallQualityScore()
        {
            var totalAnswers = await _context.Answers.CountAsync();
            if (totalAnswers == 0) return 0;

            var okAnswers = await _context.Answers.CountAsync(a => a.AnswerValue == "OK");
            return (decimal)okAnswers / totalAnswers * 100;
        }

        private decimal CalculateProjectQualityScore(Guid projectId)
        {
            var answers = _context.Checklists
                .Where(c => c.ProjectId == projectId)
                .SelectMany(c => c.Answers)
                .ToList();

            if (!answers.Any()) return 0;

            var okCount = answers.Count(a => a.AnswerValue == "OK");
            return (decimal)okCount / answers.Count * 100;
        }

        private decimal CalculateAverageNOKRate(List<Checklist.Models.Checklist> checklists)
        {
            var totalAnswers = checklists.SelectMany(c => c.Answers ?? new List<Answer>()).Count();
            if (totalAnswers == 0) return 0;

            var nokAnswers = checklists.SelectMany(c => c.Answers ?? new List<Answer>())
                .Count(a => a.AnswerValue == "NOK");
            return (decimal)nokAnswers / totalAnswers * 100;
        }

        private decimal CalculateGroupNOKRate(List<Checklist.Models.Checklist> checklists)
        {
            var totalAnswers = checklists.SelectMany(c => c.Answers ?? new List<Answer>()).Count();
            if (totalAnswers == 0) return 0;

            var nokAnswers = checklists.SelectMany(c => c.Answers ?? new List<Answer>())
                .Count(a => a.AnswerValue == "NOK");
            return (decimal)nokAnswers / totalAnswers * 100;
        }

        private decimal CalculateAvgChecklistsPerDay(List<Checklist.Models.Checklist> checklists, DateTime? start, DateTime? end)
        {
            if (!checklists.Any()) return 0;

            var startDate = start ?? checklists.Min(c => c.Date ?? DateTime.UtcNow);
            var endDate = end ?? DateTime.UtcNow;
            var days = (endDate - startDate).Days + 1;

            return days > 0 ? (decimal)checklists.Count / days : 0;
        }
    }
}