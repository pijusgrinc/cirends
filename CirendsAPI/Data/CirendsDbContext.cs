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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Configure Activity entity
            modelBuilder.Entity<Activity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Location).HasMaxLength(100);

                entity.HasOne(e => e.CreatedBy)
                      .WithMany(u => u.Activities)
                      .HasForeignKey(e => e.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure TaskItem entity
            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);

                entity.HasOne(e => e.Activity)
                      .WithMany(a => a.Tasks)
                      .HasForeignKey(e => e.ActivityId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.AssignedTo)
                      .WithMany(u => u.AssignedTasks)
                      .HasForeignKey(e => e.AssignedToUserId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.CreatedBy)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Expense entity - TIKAI Task relationship
            modelBuilder.Entity<Expense>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);

                // TIKAI Task relationship
                entity.HasOne(e => e.Task)
                      .WithMany(t => t.Expenses)
                      .HasForeignKey(e => e.TaskId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.PaidBy)
                      .WithMany(u => u.Expenses)
                      .HasForeignKey(e => e.PaidByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure ActivityUser junction table
            modelBuilder.Entity<ActivityUser>(entity =>
            {
                entity.HasKey(e => new { e.ActivityId, e.UserId });

                entity.HasOne(e => e.Activity)
                      .WithMany(a => a.ActivityUsers)
                      .HasForeignKey(e => e.ActivityId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ExpenseShare entity
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
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.ExpenseId, e.UserId }).IsUnique();
            });
        }
    }
}