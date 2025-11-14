using System.ComponentModel.DataAnnotations;

namespace Checklist.Models.Dtos
{
    public class UserCreateDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        public string? Role { get; set; } = "User";

        // 👇 List of projects the user should have access to
        public List<Guid>? ProjectIds { get; set; }

    }
}
