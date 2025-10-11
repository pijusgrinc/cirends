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
        
        public int? AssignedToUserId { get; set; }
    }

    public class TaskItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public TaskItemStatus Status { get; set; }
        public TaskItemPriority Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int ActivityId { get; set; }
        public UserDto? AssignedTo { get; set; }
        public UserDto CreatedBy { get; set; } = null!;
        public List<ExpenseDto> Expenses { get; set; } = new();
    }
}