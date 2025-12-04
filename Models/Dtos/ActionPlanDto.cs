using System;
using System.ComponentModel.DataAnnotations;

namespace Checklist.Models.Dtos
{
    public class ActionPlanCreateDto
    {
        [Required]
        public Guid QuestionId { get; set; }
        
        [Required]
        public Guid AnswerId { get; set; }
        
        // User must fill these
        [Required(ErrorMessage = "Actions are required")]
        [MinLength(5, ErrorMessage = "Actions must be at least 5 characters")]
        public string Actions { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Responsables are required")]
        [MinLength(2, ErrorMessage = "Responsables must be at least 2 characters")]
        public string Responsables { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Date de cloture is required")]
        public DateTime DateCloture { get; set; }
    }

    public class ActionPlanDto
    {
        public Guid Id { get; set; }
        public int NokPointNumber { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string Actions { get; set; } = string.Empty;
        public string Responsables { get; set; } = string.Empty;
        public DateTime DateCloture { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}