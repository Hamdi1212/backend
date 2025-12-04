using Microsoft.AspNetCore.SignalR;
using System.ComponentModel.DataAnnotations;
using System;

namespace Checklist.Models
{
    public class Checklist
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? ProjectId { get; set; } // Made nullable
        public Guid? LineId { get; set; } // Made nullable
        public Guid TemplateId { get; set; }
        public string status { get; set; } = "InProgess";

        // Existing fields
        public DateTime? Date { get; set; }
        public string? Shift { get; set; }
        public string NotificationStatus { get; set; } = "Pending";

        // NEW FIELDS - Operator Matricules
        public string? QualityOperatorMatricule { get; set; }
        public string? ProductionOperatorMatricule { get; set; }

        // Navigation properties
        public User User { get; set; }
        public Project? Project { get; set; }
        public Line? Line { get; set; }
        public Template Template { get; set; }
        public ICollection<Answer>? Answers { get; set; }
        
        // NEW - Action Plans relationship
        public ICollection<ActionPlan>? ActionPlans { get; set; }
    }
}
