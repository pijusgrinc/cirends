using CirendsAPI.Data;
using CirendsAPI.DTOs;
using CirendsAPI.Helpers;
using CirendsAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CirendsAPI.Controllers
{
    [ApiController]
    [Route("api/activities/{activityId}/tasks/{taskId}/expenses")]
    [Authorize]
    public class ExpensesController : ControllerBase
    {
        private readonly CirendsDbContext _context;
        private readonly ILogger<ExpensesController> _logger;

        public ExpensesController(CirendsDbContext context, ILogger<ExpensesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all expenses for a task (hierarchical: activity -> task -> expense)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<ExpenseDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<List<ExpenseDto>>> GetExpensesByTask(int activityId, int taskId)
        {
            var validation = ValidationHelper.ValidateHierarchyIds((activityId, "activityId"), (taskId, "taskId"));
            if (validation != null) return validation;

            try
            {
                var task = await _context.Tasks
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == taskId && t.ActivityId == activityId);

                if (task == null)
                {
                    return NotFound(new { message = "Task not found or does not belong to this activity", error = "TASK_NOT_FOUND" });
                }

                var expenses = await _context.Expenses
                    .AsNoTracking()
                    .Where(e => e.TaskId == taskId)
                    .Include(e => e.PaidBy)
                    .Include(e => e.ExpenseShares)
                    .Select(e => new ExpenseDto
                    {
                        Id = e.Id,
                        Name = e.Name,
                        Description = e.Description,
                        Amount = e.Amount,
                        Currency = e.Currency,
                        ExpenseDate = e.ExpenseDate,
                        TaskId = e.TaskId,
                        CreatedAt = e.CreatedAt,
                        UpdatedAt = e.UpdatedAt,
                        PaidBy = new UserDto
                        {
                            Id = e.PaidBy.Id,
                            Name = e.PaidBy.Name,
                            Email = e.PaidBy.Email,
                            CreatedAt = e.PaidBy.CreatedAt
                        },
                        ExpenseShares = e.ExpenseShares.Select(es => new ExpenseShareDto
                        {
                            Id = es.Id,
                            UserId = es.UserId,
                            ShareAmount = es.ShareAmount,
                            SharePercentage = es.SharePercentage,
                            IsPaid = es.IsPaid,
                            PaidAt = es.PaidAt
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(expenses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving expenses for task {TaskId}", taskId);
                return StatusCode(500, new { message = "Internal server error", error = "DATABASE_ERROR" });
            }
        }

        /// <summary>
        /// Get expense by ID (hierarchical: activity -> task -> expense)
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ExpenseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ExpenseDto>> GetExpense(int activityId, int taskId, int id)
        {
            var validation = ValidationHelper.ValidateHierarchyIds((activityId, "activityId"), (taskId, "taskId"), (id, "expenseId"));
            if (validation != null) return validation;

            try
            {
                var task = await _context.Tasks
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == taskId && t.ActivityId == activityId);

                if (task == null)
                {
                    return NotFound(new { message = "Task not found or does not belong to this activity", error = "TASK_NOT_FOUND" });
                }

                var expense = await _context.Expenses
                    .AsNoTracking()
                    .Include(e => e.PaidBy)
                    .Include(e => e.ExpenseShares)
                    .FirstOrDefaultAsync(e => e.Id == id && e.TaskId == taskId);

                if (expense == null)
                {
                    return NotFound(new { message = "Expense not found or does not belong to this task", error = "EXPENSE_NOT_FOUND" });
                }

                return Ok(new ExpenseDto
                {
                    Id = expense.Id,
                    Name = expense.Name,
                    Description = expense.Description,
                    Amount = expense.Amount,
                    Currency = expense.Currency,
                    ExpenseDate = expense.ExpenseDate,
                    TaskId = expense.TaskId,
                    CreatedAt = expense.CreatedAt,
                    UpdatedAt = expense.UpdatedAt,
                    PaidBy = new UserDto
                    {
                        Id = expense.PaidBy.Id,
                        Name = expense.PaidBy.Name,
                        Email = expense.PaidBy.Email,
                        CreatedAt = expense.PaidBy.CreatedAt
                    },
                    ExpenseShares = expense.ExpenseShares.Select(es => new ExpenseShareDto
                    {
                        Id = es.Id,
                        UserId = es.UserId,
                        ShareAmount = es.ShareAmount,
                        SharePercentage = es.SharePercentage,
                        IsPaid = es.IsPaid,
                        PaidAt = es.PaidAt
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving expense {ExpenseId}", id);
                return StatusCode(500, new { message = "Internal server error", error = "DATABASE_ERROR" });
            }
        }

        /// <summary>
        /// Create expense (hierarchical)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ExpenseDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ExpenseDto>> CreateExpense(int activityId, int taskId, [FromBody] CreateExpenseDto? createDto)
        {
            var validation = ValidationHelper.ValidateHierarchyIds((activityId, "activityId"), (taskId, "taskId"));
            if (validation != null) return validation;

            if (createDto == null)
            {
                return BadRequest(new { message = "Request body is required", error = "EMPTY_BODY" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Invalid input data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            if (!ValidationHelper.TryGetCurrentUserId(User, out var userId))
            {
                return ValidationHelper.InvalidAuthenticationResponse();
            }

            try
            {
                var task = await _context.Tasks
                    .FirstOrDefaultAsync(t => t.Id == taskId && t.ActivityId == activityId);

                if (task == null)
                {
                    return NotFound(new { message = "Task not found or does not belong to this activity", error = "TASK_NOT_FOUND" });
                }

                if (createDto.Amount <= 0)
                {
                    return BadRequest(new { message = "Expense amount must be greater than zero", error = "INVALID_AMOUNT" });
                }

                var expense = new Expense
                {
                    Name = createDto.Name.Trim(),
                    Description = createDto.Description?.Trim(),
                    Amount = createDto.Amount,
                    Currency = createDto.Currency ?? "EUR",
                    ExpenseDate = createDto.ExpenseDate,
                    TaskId = taskId,
                    PaidByUserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Expenses.Add(expense);
                await _context.SaveChangesAsync();

                // Add expense shares if provided
                if (createDto.Shares != null && createDto.Shares.Any())
                {
                    var shares = createDto.Shares.Select(s => new ExpenseShare
                    {
                        ExpenseId = expense.Id,
                        UserId = s.UserId,
                        ShareAmount = s.ShareAmount,
                        SharePercentage = s.SharePercentage ?? 0,
                        IsPaid = false,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();

                    _context.ExpenseShares.AddRange(shares);
                    await _context.SaveChangesAsync();
                }

                // Reload expense with navigation properties
                var createdExpense = await _context.Expenses
                    .Include(e => e.PaidBy)
                    .Include(e => e.ExpenseShares)
                    .FirstOrDefaultAsync(e => e.Id == expense.Id);

                return CreatedAtAction(nameof(GetExpense),
                    new { activityId, taskId, id = expense.Id },
                    MapExpenseToDto(createdExpense!));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error creating expense for task {TaskId}", taskId);
                return StatusCode(500, new { message = "Failed to create expense", error = "DATABASE_ERROR" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating expense for task {TaskId}", taskId);
                return StatusCode(500, new { message = "Internal server error", error = "UNKNOWN_ERROR" });
            }
        }

        /// <summary>
        /// Update expense
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateExpense(int activityId, int taskId, int id, [FromBody] UpdateExpenseDto? updateDto)
        {
            var validation = ValidationHelper.ValidateHierarchyIds((activityId, "activityId"), (taskId, "taskId"), (id, "expenseId"));
            if (validation != null) return validation;

            if (updateDto == null)
            {
                return BadRequest(new { message = "Request body is required", error = "EMPTY_BODY" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Invalid input data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            try
            {
                var expense = await _context.Expenses
                    .FirstOrDefaultAsync(e => e.Id == id && e.TaskId == taskId);

                if (expense == null)
                {
                    return NotFound(new { message = "Expense not found or does not belong to this task", error = "EXPENSE_NOT_FOUND" });
                }

                if (updateDto.Amount.HasValue && updateDto.Amount <= 0)
                {
                    return BadRequest(new { message = "Expense amount must be greater than zero", error = "INVALID_AMOUNT" });
                }

                if (!string.IsNullOrWhiteSpace(updateDto.Name))
                    expense.Name = updateDto.Name.Trim();

                if (updateDto.Description != null) // Allow clearing description
                    expense.Description = updateDto.Description.Trim();

                if (updateDto.Amount.HasValue)
                    expense.Amount = updateDto.Amount.Value;

                if (updateDto.ExpenseDate.HasValue)
                    expense.ExpenseDate = updateDto.ExpenseDate.Value;

                expense.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error updating expense {ExpenseId}", id);
                return StatusCode(500, new { message = "Failed to update expense", error = "DATABASE_ERROR" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating expense {ExpenseId}", id);
                return StatusCode(500, new { message = "Internal server error", error = "UNKNOWN_ERROR" });
            }
        }

        /// <summary>
        /// Delete expense
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteExpense(int activityId, int taskId, int id)
        {
            var validation = ValidationHelper.ValidateHierarchyIds((activityId, "activityId"), (taskId, "taskId"), (id, "expenseId"));
            if (validation != null) return validation;

            try
            {
                var expense = await _context.Expenses
                    .FirstOrDefaultAsync(e => e.Id == id && e.TaskId == taskId);

                if (expense == null)
                {
                    return NotFound(new { message = "Expense not found or does not belong to this task", error = "EXPENSE_NOT_FOUND" });
                }

                // Also delete related expense shares
                var shares = await _context.ExpenseShares
                    .Where(es => es.ExpenseId == id)
                    .ToListAsync();

                _context.ExpenseShares.RemoveRange(shares);
                _context.Expenses.Remove(expense);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error deleting expense {ExpenseId}", id);
                return StatusCode(500, new { message = "Failed to delete expense", error = "DATABASE_ERROR" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting expense {ExpenseId}", id);
                return StatusCode(500, new { message = "Internal server error", error = "UNKNOWN_ERROR" });
            }
        }

        /// <summary>
        /// Helper method to map Expense to ExpenseDto
        /// </summary>
        private ExpenseDto MapExpenseToDto(Expense expense)
        {
            return new ExpenseDto
            {
                Id = expense.Id,
                Name = expense.Name,
                Description = expense.Description,
                Amount = expense.Amount,
                Currency = expense.Currency,
                ExpenseDate = expense.ExpenseDate,
                TaskId = expense.TaskId,
                CreatedAt = expense.CreatedAt,
                UpdatedAt = expense.UpdatedAt,
                PaidBy = new UserDto
                {
                    Id = expense.PaidBy.Id,
                    Name = expense.PaidBy.Name,
                    Email = expense.PaidBy.Email,
                    CreatedAt = expense.PaidBy.CreatedAt
                },
                ExpenseShares = expense.ExpenseShares?.Select(es => new ExpenseShareDto
                {
                    Id = es.Id,
                    UserId = es.UserId,
                    ShareAmount = es.ShareAmount,
                    SharePercentage = es.SharePercentage,
                    IsPaid = es.IsPaid,
                    PaidAt = es.PaidAt
                }).ToList() ?? new List<ExpenseShareDto>()
            };
        }
    }

    public class CreateExpenseDto
    {
        [Required(ErrorMessage = "Expense name is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 200 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description must not exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }

        [StringLength(3, ErrorMessage = "Currency code must be 3 characters")]
        public string? Currency { get; set; }

        [Required(ErrorMessage = "Expense date is required")]
        public DateTime ExpenseDate { get; set; }

        public List<ExpenseShareRequest> Shares { get; set; } = new();
    }

    public class UpdateExpenseDto
    {
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 200 characters")]
        public string? Name { get; set; }

        [StringLength(500, ErrorMessage = "Description must not exceed 500 characters")]
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal? Amount { get; set; }

        public DateTime? ExpenseDate { get; set; }
    }

    public class ExpenseShareRequest
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int UserId { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal ShareAmount { get; set; }

        [Range(0, 100)]
        public decimal? SharePercentage { get; set; }
    }
}