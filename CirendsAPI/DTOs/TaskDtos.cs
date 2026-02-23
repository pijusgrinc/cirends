using System.ComponentModel.DataAnnotations;
using CirendsAPI.Models;

namespace CirendsAPI.DTOs
{
    public class CreateTaskDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        public DateTime? DueDate { get; set; }
        
        public TaskItemPriority Priority { get; set; } = TaskItemPriority.Medium;
        
        [Required]
        public int? AssignedToUserId { get; set; }
    }

    public class UpdateTaskDto
    {
        [StringLength(200)]
        public string? Name { get; set; }
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        public DateTime? DueDate { get; set; }
        
        public TaskItemStatus? Status { get; set; }
        
        public TaskItemPriority? Priority { get; set; }

        [Required]
        public int? AssignedToUserId { get; set; }
    }

    public class TaskDto
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        [Required]
        public int Status { get; set; }
        [Required]
        public int Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        [Required]
        public int ActivityId { get; set; }
    }

    public class TaskItemDto
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        [StringLength(1000)]
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        [Required]
        public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;
        [Required]
        public TaskItemPriority Priority { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public DateTime UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        [Required]
        public int ActivityId { get; set; }
        public UserDto? AssignedTo { get; set; }
        public UserDto CreatedBy { get; set; } = null!;
    }
}