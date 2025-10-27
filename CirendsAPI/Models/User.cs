using System.ComponentModel.DataAnnotations;

namespace CirendsAPI.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(50)]
        public string Role { get; set; } = "User";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Activity> ActivitiesCreated { get; set; } = new List<Activity>();

        public ICollection<ActivityUser> ActivityUsers { get; set; } = new List<ActivityUser>();
        public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
        public ICollection<TaskItem> CreatedTasks { get; set; } = new List<TaskItem>();
        public ICollection<Expense> ExpensesPaid { get; set; } = new List<Expense>();
        public ICollection<ExpenseShare> ExpenseShares { get; set; } = new List<ExpenseShare>();
        public ICollection<Invitation> InvitationsSent { get; set; } = new List<Invitation>();
        public ICollection<Invitation> InvitationsReceived { get; set; } = new List<Invitation>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}