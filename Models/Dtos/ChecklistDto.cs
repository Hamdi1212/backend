using System;
using System.Collections.Generic;

namespace Checklist.Models.Dtos
{
    public class ChecklistDto
    {
        public Guid Id { get; set; }
<<<<<<< HEAD
=======
        public string Title { get; set; } = string.Empty;
>>>>>>> 9969a264276429e8a85024f3516fd8aee3b2d15a
        public string TemplateName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public List<AnswerDto> Answers { get; set; } = new();
    }
}