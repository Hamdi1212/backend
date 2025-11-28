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

        [Required]
        public List<AnswerSubmitDto> Answers { get; set; } = new();
    }

    public class AnswerSubmitDto
    {
        [Required]
        public Guid QuestionId { get; set; }

        [Required]
        public string AnswerValue { get; set; } = string.Empty;
    }
}