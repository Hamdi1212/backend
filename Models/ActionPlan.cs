using System;
using System.ComponentModel.DataAnnotations;

namespace Checklist.Models
{
    public class ActionPlan
    {
        [Key]
        public Guid Id { get; set; }
        
        // Links to related entities
        public Guid ChecklistId { get; set; }
        public Guid AnswerId { get; set; }
        public Guid QuestionId { get; set; }
        
        // Auto-generated fields
        public int NokPointNumber { get; set; }           // Question number (1, 2, 3...)
        public DateTime CreatedDate { get; set; }         // When action plan was created
        public string CreatedBy { get; set; } = string.Empty;  // Username who created it
        
        // User-filled fields (REQUIRED)
        [Required]
        public string Actions { get; set; } = string.Empty;  // Corrective actions to take
        
        [Required]
        public string Responsables { get; set; } = string.Empty;  // Responsible person(s)
        
        [Required]
        public DateTime DateCloture { get; set; }         // Completion deadline
        
        // Status tracking
        public string Status { get; set; } = "Open";      // Open/InProgress/Closed
        
        // Navigation properties
        public Checklist Checklist { get; set; }
        public Answer Answer { get; set; }
        public Question Question { get; set; }
    }
}