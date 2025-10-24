using System.ComponentModel.DataAnnotations.Schema;

namespace CirendsAPI.Models
{
    // Junction table for Activity-User many-to-many relationship

    public class ActivityUser
    {
            public int ActivityId { get; set; }

            public int UserId { get; set; }

            public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

            public bool IsAdmin { get; set; } = false;

            // Navigation properties
            [ForeignKey("ActivityId")]
            public Activity? Activity { get; set; }

            [ForeignKey("UserId")]
            public User? User { get; set; }
    }

    // Junction table for expense sharing
    public class ExpenseShare
    {
        public int Id { get; set; }

        public int ExpenseId { get; set; }

        public int UserId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ShareAmount { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal SharePercentage { get; set; }

        public bool IsPaid { get; set; } = false;

        public DateTime? PaidAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ExpenseId")]
        public Expense? Expense { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}