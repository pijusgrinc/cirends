using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CirendsAPI.Models
{
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
        Urgent = 3
    }

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
        [ForeignKey("ActivityId")]
        public Activity? Activity { get; set; }

        [ForeignKey("AssignedToUserId")]
        public User? AssignedTo { get; set; }

        [ForeignKey("CreatedByUserId")]
        public User? CreatedBy { get; set; }

    }
}