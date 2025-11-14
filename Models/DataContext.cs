using Microsoft.EntityFrameworkCore;

namespace Checklist.Models
{
    public class DataContext : DbContext

    {
        
        public DataContext(DbContextOptions opt) : base(opt) {}
        
        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Line> Lines { get; set; }  
        public DbSet<Template> Templates { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Checklist> Checklists { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<UserProject> UserProjects { get; set; } // <-- Add this line
    }
}
        
