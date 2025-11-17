using System;
using System.Collections.Generic;

namespace Checklist.Models.Dtos
{
    public class ChecklistDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string TemplateName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public List<AnswerDto> Answers { get; set; } = new();
    }
}