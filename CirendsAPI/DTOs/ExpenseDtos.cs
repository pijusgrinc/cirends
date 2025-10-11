using System.ComponentModel.DataAnnotations;

namespace CirendsAPI.DTOs
{
    public class CreateExpenseDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
        
        [StringLength(3)]
        public string Currency { get; set; } = "EUR";
        
        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
        
        public int? TaskId { get; set; }
        
        public List<ExpenseShareDto> Shares { get; set; } = new();
    }

    public class UpdateExpenseDto
    {
        [StringLength(200)]
        public string? Name { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal? Amount { get; set; }
        
        [StringLength(3)]
        public string? Currency { get; set; }
        
        public DateTime? ExpenseDate { get; set; }
        
        public int? TaskId { get; set; }
    }

    public class ExpenseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EUR";
        public DateTime ExpenseDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int ActivityId { get; set; }
        public int? TaskId { get; set; }
        public UserDto PaidBy { get; set; } = null!;
        public List<ExpenseShareDto> ExpenseShares { get; set; } = new();
    }

    public class ExpenseShareDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public UserDto? User { get; set; }
        public decimal ShareAmount { get; set; }
        public decimal SharePercentage { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}