using System;
using System.ComponentModel.DataAnnotations;

namespace Checklist.Models.Dtos
{
    public class ChecklistStartDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid TemplateId { get; set; }

        public Guid? ProjectId { get; set; }
        
        public Guid? LineId { get; set; }

        public DateTime? Date { get; set; }

        public string? Shift { get; set; }
    }

    public class ChecklistSubmitDto
    {
        [Required]
        public Guid ChecklistId { get; set; }

        // NEW - Operator Matricules (REQUIRED, numbers only)
        [Required(ErrorMessage = "Quality Operator Matricule is required")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Quality Operator Matricule must contain only numbers")]
        public string QualityOperatorMatricule { get; set; } = string.Empty;

        [Required(ErrorMessage = "Production Operator Matricule is required")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Production Operator Matricule must contain only numbers")]
        public string ProductionOperatorMatricule { get; set; } = string.Empty;

        [Required]
        public List<AnswerSubmitDto> Answers { get; set; } = new();

        // NEW - Action Plans for NOK answers
        public List<ActionPlanCreateDto>? ActionPlans { get; set; }
    }

    public class AnswerSubmitDto
    {
        [Required]
        public Guid QuestionId { get; set; }

        [Required]
        public string AnswerValue { get; set; } = string.Empty;
    }
}