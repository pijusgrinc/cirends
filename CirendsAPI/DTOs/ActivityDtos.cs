using System.ComponentModel.DataAnnotations;

namespace CirendsAPI.DTOs
{
    public class CreateActivityDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }
    }

    public class UpdateActivityDto
    {
        [StringLength(200)]
        public string? Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }
    }

    public class ActivityDto
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [StringLength(1000)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public UserDto CreatedBy { get; set; } = null!;
        public List<TaskItemDto> Tasks { get; set; } = new();
        public List<ActivityUserDto> Participants { get; set; } = new(); // Changed from List<UserDto> to List<ActivityUserDto>
    }

    // Add the missing ActivityUserDto class
    public class ActivityUserDto
    {
        [Key]
        public int ActivityId { get; set; }

        public int UserId { get; set; }

        public bool IsAdmin { get; set; }

        public DateTime JoinedAt { get; set; }
        public UserDto User { get; set; } = null!;
    }
}