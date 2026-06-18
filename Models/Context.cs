using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Task_Management_Project.Models
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // ===== ALL RELATIONSHIPS COMMENTED - Uncomment when needed =====

            /*

            // ===== User Relationships =====
            //m : m between team and user via teammember
            modelBuilder.Entity<TeamMember>()
                .HasKey(tm => new { tm.UserId, tm.TeamId });

            modelBuilder.Entity<TeamMember>()
                .HasOne(tm => tm.User)
                .WithMany(u => u.TeamMembers)
                .HasForeignKey(tm => tm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TeamMember>()
                .HasOne(tm => tm.Team)
                .WithMany(t => t.TeamMembers)
                .HasForeignKey(tm => tm.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            */
            // ===== Project Relationships =====
            // Project - User => Many : One
            modelBuilder.Entity<Project>()
                .HasOne(p => p.CreatedByUser)
                .WithMany(u => u.Projects)
                .HasForeignKey(p => p.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Project - TaskItems =>  One : Many
            modelBuilder.Entity<Project>()
                .HasMany(p => p.TaskItems)
                .WithOne(t => t.Project)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            /*
                        // Project - Team => Many : One
                        modelBuilder.Entity<Project>()
                            .HasOne(p => p.Team)
                            .WithMany(t => t.Projects)
                            .HasForeignKey(p => p.TeamId)
                            .OnDelete(DeleteBehavior.Restrict);

                 */

            // ===== TaskItem Relationships =====
            // TaskItem - User => Many : One 

            // Creator relation           
            modelBuilder.Entity<TaskItem>()
           .HasOne(t => t.CreatedByUser)
           .WithMany(u => u.TasksCreated)
           .HasForeignKey(t => t.CreatedByUserId)
           .OnDelete(DeleteBehavior.Restrict);


            // Assigned relation
            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.AssignedToUser)
                .WithMany(u => u.TasksAssigned)
                .HasForeignKey(t => t.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull); // If a user is deleted, we set AssignedToUserId to null instead of deleting the task



            // TaskItem - Comments => One : Many
            modelBuilder.Entity<TaskItem>()
                .HasMany(t => t.Comments)
                .WithOne(c => c.Task)
                .HasForeignKey(c => c.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== Comment Relationships =====
            // Comment - User => Many : One
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== Notification Relationships =====
            // Notification - User => Many : One
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            

            // Indexes for performance
            modelBuilder.Entity<User>()
    .HasIndex(u => u.Email)
    .IsUnique();

modelBuilder.Entity<User>()
    .HasIndex(u => u.Username)
    .IsUnique();

modelBuilder.Entity<TaskItem>()
    .HasIndex(t => t.Status);

modelBuilder.Entity<TaskItem>()
    .HasIndex(t => t.DueDate);

            modelBuilder.Entity<TaskItem>()
                .HasIndex(t => t.ProjectId);

            modelBuilder.Entity<Project>()
                .HasIndex(p => p.CreatedByUserId);

            modelBuilder.Entity<Comment>()
    .HasIndex(c => c.TaskId);

            modelBuilder.Entity<Comment>()
                .HasIndex(c => c.UserId);

            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.UserId, n.IsRead });







        }
    }
}
