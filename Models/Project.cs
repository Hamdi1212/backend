using System.ComponentModel.DataAnnotations;

namespace Checklist.Models
{
    public class Project
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<Line>? Lines { get; set; }
        public ICollection<Checklist>? Checklists { get; set; }

    }
}
