using Microsoft.EntityFrameworkCore;
using CirendsAPI.Models;

namespace CirendsAPI.Data
{
    public class CirendsDbContext : DbContext
    {
        public CirendsDbContext(DbContextOptions<CirendsDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<ActivityUser> ActivityUsers { get; set; }
        public DbSet<ExpenseShare> ExpenseShares { get; set; }
        public DbSet<Invitation> Invitations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Role).HasMaxLength(50);
            });

            // Activity
            modelBuilder.Entity<Activity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Location).HasMaxLength(100);

                entity.HasOne(e => e.CreatedBy)
                    .WithMany(u => u.ActivitiesCreated)
                    .HasForeignKey(e => e.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // TaskItem
            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Status).HasConversion<int>();
                entity.Property(e => e.Priority).HasConversion<int>();

                entity.HasOne(e => e.Activity)
                    .WithMany(a => a.Tasks)
                    .HasForeignKey(e => e.ActivityId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.AssignedTo)
                    .WithMany(u => u.AssignedTasks)
                    .HasForeignKey(e => e.AssignedToUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.CreatedBy)
                    .WithMany(u => u.CreatedTasks)
                    .HasForeignKey(e => e.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Expense
            modelBuilder.Entity<Expense>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);

                entity.HasOne(e => e.Task)
                    .WithMany(t => t.Expenses)
                    .HasForeignKey(e => e.TaskId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.PaidBy)
                    .WithMany(u => u.ExpensesPaid)
                    .HasForeignKey(e => e.PaidByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ActivityUser
            modelBuilder.Entity<ActivityUser>(entity =>
            {
                entity.HasKey(e => new { e.ActivityId, e.UserId });

                entity.HasOne(e => e.Activity)
                    .WithMany(a => a.ActivityUsers)
                    .HasForeignKey(e => e.ActivityId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.ActivityUsers)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ExpenseShare
            modelBuilder.Entity<ExpenseShare>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ShareAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.SharePercentage).HasColumnType("decimal(5,2)");

                entity.HasOne(e => e.Expense)
                    .WithMany(ex => ex.ExpenseShares)
                    .HasForeignKey(e => e.ExpenseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.ExpenseShares)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.ExpenseId, e.UserId }).IsUnique();
            });

            // Invitation
            modelBuilder.Entity<Invitation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).HasConversion<int>();
                entity.Property(e => e.Message).HasMaxLength(500);

                entity.HasOne(e => e.Activity)
                    .WithMany(a => a.Invitations)
                    .HasForeignKey(e => e.ActivityId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.InvitedBy)
                    .WithMany(u => u.InvitationsSent)
                    .HasForeignKey(e => e.InvitedUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.InvitedUser)
                    .WithMany(u => u.InvitationsReceived)
                    .HasForeignKey(e => e.InvitedUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}