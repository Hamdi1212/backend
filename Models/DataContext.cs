using Microsoft.EntityFrameworkCore;

namespace Checklist.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Line> Lines { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Checklist> Checklists { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<UserProject> UserProjects { get; set; }

        // Configure relationships and prevent multiple cascade paths
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<UserProject>()
        .HasKey(up => new { up.UserId, up.ProjectId });

            modelBuilder.Entity<UserProject>()
                .HasOne(up => up.User)
                .WithMany(u => u.UserProjects)
                .HasForeignKey(up => up.UserId);

            modelBuilder.Entity<UserProject>()
                .HasOne(up => up.Project)
                .WithMany(p => p.UserProjects)
                .HasForeignKey(up => up.ProjectId);

            // Checklist ↔ Project
            modelBuilder.Entity<Checklist>()
                .HasOne(c => c.Project)
                .WithMany()
                .HasForeignKey(c => c.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // Checklist ↔ Line
            modelBuilder.Entity<Checklist>()
                .HasOne(c => c.Line)
                .WithMany()
                .HasForeignKey(c => c.LineId)
                .OnDelete(DeleteBehavior.Restrict);

            // Checklist ↔ Template
            modelBuilder.Entity<Checklist>()
                .HasOne(c => c.Template)
                .WithMany()
                .HasForeignKey(c => c.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            // Checklist ↔ User
            modelBuilder.Entity<Checklist>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
