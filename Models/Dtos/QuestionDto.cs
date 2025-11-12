using System;

namespace Checklist.Models.Dtos
{
    public class QuestionDto
    {
        public Guid Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
       
        public Guid TemplateId { get; set; }
        public string? TemplateName { get; set; }
    }

    public class QuestionCreateDto
    {
        public string QuestionText { get; set; } = string.Empty;
        
        public Guid TemplateId { get; set; }
    }

    public class QuestionUpdateDto
    {
        public string QuestionText { get; set; } = string.Empty;
        public Guid TemplateId { get; set; }

    }
}