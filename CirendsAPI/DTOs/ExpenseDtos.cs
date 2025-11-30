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
        
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        
        [StringLength(3)]
        public string Currency { get; set; } = "EUR";
        
        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
        [Required]
        [Range(1, int.MaxValue)]
        public int PaidByUserId { get; set; }
        
        public List<ExpenseShareRequest> Shares { get; set; } = new();
    }

    public class UpdateExpenseDto
    {
        [StringLength(200)]
        public string? Name { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Range(0.01, double.MaxValue)]
        public decimal? Amount { get; set; }
        
        [StringLength(3)]
        public string? Currency { get; set; }
        
        public DateTime? ExpenseDate { get; set; }
        [Range(1, int.MaxValue)]
        public int? PaidByUserId { get; set; }
        public List<ExpenseShareRequest>? Shares { get; set; }
    }

    public class ExpenseDto
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        [StringLength(500)]
        public string? Description { get; set; }
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "EUR";
        public DateTime ExpenseDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        [Required]
        public int TaskId { get; set; }
        [Required]
        public int PaidByUserId { get; set; }
        public UserDto? PaidBy { get; set; }
        public List<ExpenseShareDto> ExpenseShares { get; set; } = new();
    }

    public class ExpenseShareDto
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal ShareAmount { get; set; }
        [Required]
        [Range(0, 100)]
        public decimal SharePercentage { get; set; }  // Added this property
        [Required]
        public bool IsPaid { get; set; }
        public DateTime? PaidAt { get; set; }  // Added this property
        [Required]
        public UserDto? User { get; set; }
        
    }
    public class CreateExpenseShareDto
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal ShareAmount { get; set; }
    }

    public class ExpenseShareRequest
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal ShareAmount { get; set; }
        [Range(0, 100)]
        public decimal? SharePercentage { get; set; }
    }
}