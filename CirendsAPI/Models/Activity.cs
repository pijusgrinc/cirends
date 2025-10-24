using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CirendsAPI.Models
{
    public class Activity
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key
        public int CreatedByUserId { get; set; }

        // Navigation properties
        [ForeignKey("CreatedByUserId")]
        public User? CreatedBy { get; set; }

        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
        public ICollection<ActivityUser> ActivityUsers { get; set; } = new List<ActivityUser>();
        public ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();
    }
}