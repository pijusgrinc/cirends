using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CirendsAPI.Models
{
    public enum InvitationStatus
    {
        Pending = 0,
        Accepted = 1,
        Rejected = 2,
        Cancelled = 3
    }

    public class Invitation
    {
        public int Id { get; set; }

        public int ActivityId { get; set; }

        public int InvitedByUserId { get; set; }

        public int InvitedUserId { get; set; }

        public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RespondedAt { get; set; }

        [StringLength(500)]
        public string? Message { get; set; }

        // Navigation properties
        [ForeignKey("ActivityId")]
        public Activity? Activity { get; set; }

        [ForeignKey("InvitedByUserId")]
        public User? InvitedBy { get; set; }

        [ForeignKey("InvitedUserId")]
        public User? InvitedUser { get; set; }
    }
}