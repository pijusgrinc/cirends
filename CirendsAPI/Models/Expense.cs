using System.ComponentModel.DataAnnotations;

namespace CirendsAPI.Models
{
    public class Expense
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        
        [StringLength(3)]
        public string Currency { get; set; } = "EUR";
        
        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign keys
        public int TaskId { get; set; }  // Required
        public int PaidByUserId { get; set; }
        
        // Navigation properties
        public TaskItem Task { get; set; } = null!;
        public User PaidBy { get; set; } = null!;
        public List<ExpenseShare> ExpenseShares { get; set; } = new();
    }
}