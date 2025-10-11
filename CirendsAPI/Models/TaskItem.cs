using System.ComponentModel.DataAnnotations;

namespace CirendsAPI.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        public DateTime? DueDate { get; set; }
        
        public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;
        
        public TaskItemPriority Priority { get; set; } = TaskItemPriority.Medium;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        
        // Foreign keys
        public int ActivityId { get; set; }
        public int? AssignedToUserId { get; set; }
        public int CreatedByUserId { get; set; }
        
        // Navigation properties
        public Activity Activity { get; set; } = null!;
        public User? AssignedTo { get; set; }
        public User CreatedBy { get; set; } = null!;
        public List<Expense> Expenses { get; set; } = new();
    }
    
    public enum TaskItemStatus
    {
        Pending = 0,
        InProgress = 1,
        Completed = 2,
        Cancelled = 3
    }
    
    public enum TaskItemPriority
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Critical = 3
    }
}