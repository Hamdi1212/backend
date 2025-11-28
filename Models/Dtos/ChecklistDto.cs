using System;
using System.Collections.Generic;

namespace Checklist.Models.Dtos
{
    public class ChecklistDto
    {
        public Guid Id { get; set; }

        
        public string TemplateName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public List<AnswerDto> Answers { get; set; } = new();

        // Add navigation properties
        public TemplateDto? Template { get; set; }
        public ProjectDto? Project { get; set; }
        public LineDto? Line { get; set; }
    }
}