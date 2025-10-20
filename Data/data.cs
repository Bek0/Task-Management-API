using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Models;

namespace TaskManagement.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Models.Task> Tasks { get; set; }
        public DbSet<TaskUpdate> TaskUpdates { get; set; }
        public DbSet<TaskReview> TaskReviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserID);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            // Task Configuration
            modelBuilder.Entity<Models.Task>(entity =>
            {
                entity.HasKey(t => t.TaskID);
                entity.Property(t => t.Status).HasDefaultValue("Pending");
                entity.Property(t => t.CreatedAt).HasDefaultValueSql("GETDATE()");

                // CreatedBy relationship
                entity.HasOne(t => t.Creator)
                    .WithMany(u => u.CreatedTasks)
                    .HasForeignKey(t => t.CreatedBy)
                    .OnDelete(DeleteBehavior.Cascade);

                // AssignedTo relationship
                entity.HasOne(t => t.AssignedUser)
                    .WithMany(u => u.AssignedTasks)
                    .HasForeignKey(t => t.AssignedTo)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // TaskUpdate Configuration
            modelBuilder.Entity<TaskUpdate>(entity =>
            {
                entity.HasKey(tu => tu.UpdateID);
                entity.Property(tu => tu.UpdateDate).HasDefaultValueSql("GETDATE()");

                entity.HasOne(tu => tu.Task)
                    .WithMany(t => t.TaskUpdates)
                    .HasForeignKey(tu => tu.TaskID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(tu => tu.User)
                    .WithMany(u => u.TaskUpdates)
                    .HasForeignKey(tu => tu.UpdatedBy)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // TaskReview Configuration
            modelBuilder.Entity<TaskReview>(entity =>
            {
                entity.HasKey(tr => tr.ReviewID);
                entity.Property(tr => tr.ReviewDate).HasDefaultValueSql("GETDATE()");

                entity.HasOne(tr => tr.Task)
                    .WithMany(t => t.TaskReviews)
                    .HasForeignKey(tr => tr.TaskID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(tr => tr.Reviewer)
                    .WithMany(u => u.TaskReviews)
                    .HasForeignKey(tr => tr.ReviewedBy)
                    .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}