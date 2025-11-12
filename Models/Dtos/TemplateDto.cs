using System;

namespace Checklist.Models.Dtos
{
    public class TemplateDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class TemplateCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class TemplateUpdateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}