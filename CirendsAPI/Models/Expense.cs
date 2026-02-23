using CirendsAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    [Required]
    [StringLength(3)]
    public string Currency { get; set; } = "EUR";

    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Foreign keys
    public int ActivityId { get; set; }

    public int PaidByUserId { get; set; }

    // Navigation properties
    [ForeignKey("ActivityId")]
    public Activity? Activity { get; set; }

    [ForeignKey("PaidByUserId")]
    public User? PaidBy { get; set; }

    public ICollection<ExpenseShare> ExpenseShares { get; set; } = new List<ExpenseShare>();
}