using System;
using System.Collections.Generic;

namespace Checklist.Models.Dtos
{
    public class ChecklistCreateDto
    {
        public Guid TemplateId { get; set; }
        public Guid UserId { get; set; }
        public List<AnswerDto> Answers { get; set; } = new();
    }

    public class AnswerDto
    {
        public Guid QuestionId { get; set; }
        public string Response { get; set; } = string.Empty;
    }
}