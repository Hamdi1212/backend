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
        public Guid ProjectId { get; set; }
        public Guid LineId { get; set; }
        public Guid TemplateId { get; set; }
        public string status { get; set; } = "InProgess";

        // New fields requested: Date, Shift, NotificationStatus
        // Date: timestamp for creation/submission
        public DateTime? Date { get; set; }

        // Shift for header information from the front
        public string? Shift { get; set; }

        // Notification status for PDF/email processing
        public string NotificationStatus { get; set; } = "Pending";

        // object navigation properties
        public User User { get; set; }
        public Project Project { get; set; }

        public Line Line { get; set; }
        public Template Template { get; set; }

        public ICollection<Answer>? Answers { get; set; }
    }
}
