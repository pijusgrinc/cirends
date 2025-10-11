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
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public List<Activity> Activities { get; set; } = new();
        public List<TaskItem> AssignedTasks { get; set; } = new();
        public List<Expense> Expenses { get; set; } = new();
    }
}