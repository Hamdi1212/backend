using System.ComponentModel.DataAnnotations;

namespace Checklist.Models
{
    public class Line
    {

        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid ProjectId { get; set; }
        [Required]
        public Project Project { get; set; }
        public ICollection<Checklist>? Checklists { get; set; }
    }
}
