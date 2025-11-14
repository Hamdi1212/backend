using System.ComponentModel.DataAnnotations;

namespace Checklist.Models
{
    public class Template
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<Question> ?questions { get; set; }
        public ICollection<Checklist>? Checklists { get; set; }
    }
}
