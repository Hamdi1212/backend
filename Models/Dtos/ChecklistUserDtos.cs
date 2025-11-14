using System;
using System.Collections.Generic;

namespace Checklist.Models.Dtos
{
    public class ChecklistStartDto
    {
        // keep UserId for now, but recommend deriving from JWT in controller
        public Guid UserId { get; set; }
        public Guid ProjectId { get; set; }
        public Guid LineId { get; set; }
        public Guid TemplateId { get; set; }

        // New header fields from front
        public DateTime? Date { get; set; }
        public string? Shift { get; set; }
    }

    public class ChecklistSubmitDto
    {
        public Guid ChecklistId { get; set; }
        public List<UserAnswerDto> Answers { get; set; } = new();
    }

    public class UserAnswerDto
    {
        public Guid QuestionId { get; set; }
        public string AnswerValue { get; set; } = string.Empty;
    }
}