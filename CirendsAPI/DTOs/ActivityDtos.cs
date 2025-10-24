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
        
        public DateTime StartDate { get; set; }
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
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Location { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public UserDto? CreatedBy { get; set; }
        public List<TaskDto> Tasks { get; set; } = new();
        public List<ActivityUserDto> Participants { get; set; } = new();
    }

    public class ActivityUserDto
    {
        public int ActivityId { get; set; }
        public int UserId { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime JoinedAt { get; set; }
        public UserDto? User { get; set; }
    }
}