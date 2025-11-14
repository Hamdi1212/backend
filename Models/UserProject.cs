using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Checklist.Models

{
    public class UserProject
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } 
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } 
    }
}
