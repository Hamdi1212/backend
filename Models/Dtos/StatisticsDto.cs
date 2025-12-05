namespace Checklist.Models.Dtos
{
    public class ProjectStatisticsDto
    {
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        
        // Overall Statistics
        public int TotalChecklists { get; set; }
        public int CompletedChecklists { get; set; }
        public int PendingChecklists { get; set; }
        public decimal CompletionRate { get; set; }
        
        // Answer Statistics
        public int TotalAnswers { get; set; }
        public int TotalOKAnswers { get; set; }
        public int TotalNOKAnswers { get; set; }
        public decimal OKRate { get; set; }
        public decimal NOKRate { get; set; }
        
        // Action Plan Statistics
        public int TotalActionPlans { get; set; }
        public int OpenActionPlans { get; set; }
        public int ClosedActionPlans { get; set; }
        public decimal ActionPlanClosureRate { get; set; }
        
        // Time-based data for charts
        public List<ChecklistTrendData> ChecklistTrends { get; set; } = new();
        public List<TemplateUsageData> TemplateUsage { get; set; } = new();
        public List<LinePerformanceData> LinePerformance { get; set; } = new();
        public List<UserProductivityData> UserProductivity { get; set; } = new();
        public List<ShiftDistributionData> ShiftDistribution { get; set; } = new();
    }

    public class ChecklistTrendData
    {
        public DateTime Date { get; set; }
        public int TotalChecklists { get; set; }
        public int CompletedChecklists { get; set; }
        public int NOKCount { get; set; }
    }

    public class TemplateUsageData
    {
        public string TemplateName { get; set; } = string.Empty;
        public int UsageCount { get; set; }
        public decimal AverageNOKRate { get; set; }
    }

    public class LinePerformanceData
    {
        public string LineName { get; set; } = string.Empty;
        public int TotalChecklists { get; set; }
        public int NOKCount { get; set; }
        public decimal NOKRate { get; set; }
    }

    public class UserProductivityData
    {
        public string UserName { get; set; } = string.Empty;
        public int ChecklistsCompleted { get; set; }
        public int ActionPlansCreated { get; set; }
    }

    public class ShiftDistributionData
    {
        public string Shift { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal NOKRate { get; set; }
    }

    public class DashboardOverviewDto
    {
        public int TotalProjects { get; set; }
        public int TotalUsers { get; set; }
        public int TotalTemplates { get; set; }
        public int TotalChecklists { get; set; }
        public int TodayChecklists { get; set; }
        public int PendingActionPlans { get; set; }
        public decimal OverallQualityScore { get; set; }
        
        public List<ProjectSummaryData> TopProjects { get; set; } = new();
        public List<RecentActivityData> RecentActivities { get; set; } = new();
    }

    public class ProjectSummaryData
    {
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public int ChecklistCount { get; set; }
        public decimal QualityScore { get; set; }
    }

    public class RecentActivityData
    {
        public DateTime Date { get; set; }
        public string Activity { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
    }
}