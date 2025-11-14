using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Checklist.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string password { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        public ICollection<Checklist>? Checklists { get; set; } 




    }
}
