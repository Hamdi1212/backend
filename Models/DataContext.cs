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
        public DbSet<UserProject> UserProjects { get; set; }
        public DbSet<ActionPlan> ActionPlans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Checklist relationships
            modelBuilder.Entity<Checklist>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Checklist>()
                .HasOne(c => c.Template)
                .WithMany()
                .HasForeignKey(c => c.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Checklist>()
                .HasOne(c => c.Project)
                .WithMany()
                .HasForeignKey(c => c.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Checklist>()
                .HasOne(c => c.Line)
                .WithMany()
                .HasForeignKey(c => c.LineId)
                .OnDelete(DeleteBehavior.Restrict);

            // Answer relationships
            modelBuilder.Entity<Answer>()
                .HasOne(a => a.Checklist)
                .WithMany(c => c.Answers)
                .HasForeignKey(a => a.ChecklistId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Answer>()
                .HasOne(a => a.Question)
                .WithMany(q => q.Answers)  // ✅ Specify the inverse navigation
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Question relationships
            modelBuilder.Entity<Question>()
                .HasOne(q => q.Template)
                .WithMany()
                .HasForeignKey(q => q.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            // ActionPlan relationships
            modelBuilder.Entity<ActionPlan>()
                .HasOne(ap => ap.Checklist)
                .WithMany(c => c.ActionPlans)
                .HasForeignKey(ap => ap.ChecklistId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ActionPlan>()
                .HasOne(ap => ap.Answer)
                .WithMany()
                .HasForeignKey(ap => ap.AnswerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ActionPlan>()
                .HasOne(ap => ap.Question)
                .WithMany()
                .HasForeignKey(ap => ap.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
