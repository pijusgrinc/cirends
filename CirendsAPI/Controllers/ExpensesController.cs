using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CirendsAPI.Data;
using CirendsAPI.DTOs;
using CirendsAPI.Models;
using System.Security.Claims;

namespace CirendsAPI.Controllers
{
    [ApiController]
    [Route("api/activities/{activityId}/[controller]")]
    [Authorize]
    public class ExpensesController : ControllerBase
    {
        private readonly CirendsDbContext _context;

        public ExpensesController(CirendsDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExpenseDto>>> GetExpenses(int activityId)
        {
            var userId = GetCurrentUserId();
            
            // Check if user has access to this activity
            var hasAccess = await _context.Activities
                .AnyAsync(a => a.Id == activityId && (a.CreatedByUserId == userId || a.ActivityUsers.Any(au => au.UserId == userId)));

            if (!hasAccess)
            {
                return NotFound("Activity not found or no access");
            }

            var expenses = await _context.Expenses
                .Include(e => e.PaidBy)
                .Include(e => e.Task)
                .Include(e => e.ExpenseShares)
                .ThenInclude(es => es.User)
                .Where(e => e.ActivityId == activityId)
                .ToListAsync();

            var expenseDtos = expenses.Select(e => new ExpenseDto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                Amount = e.Amount,
                Currency = e.Currency,
                ExpenseDate = e.ExpenseDate,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
                ActivityId = e.ActivityId,
                TaskId = e.TaskId,
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
                    User = new UserDto
                    {
                        Id = es.User.Id,
                        Name = es.User.Name,
                        Email = es.User.Email,
                        CreatedAt = es.User.CreatedAt
                    },
                    ShareAmount = es.ShareAmount,
                    SharePercentage = es.SharePercentage,
                    IsPaid = es.IsPaid,
                    PaidAt = es.PaidAt
                }).ToList()
            }).ToList();

            return Ok(expenseDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ExpenseDto>> GetExpense(int activityId, int id)
        {
            var userId = GetCurrentUserId();
            
            var expense = await _context.Expenses
                .Include(e => e.PaidBy)
                .Include(e => e.Task)
                .Include(e => e.Activity)
                .ThenInclude(a => a.ActivityUsers)
                .Include(e => e.ExpenseShares)
                .ThenInclude(es => es.User)
                .Where(e => e.Id == id && e.ActivityId == activityId)
                .FirstOrDefaultAsync();

            if (expense == null)
            {
                return NotFound();
            }

            // Check if user has access to this activity
            if (expense.Activity.CreatedByUserId != userId && !expense.Activity.ActivityUsers.Any(au => au.UserId == userId))
            {
                return NotFound();
            }

            var expenseDto = new ExpenseDto
            {
                Id = expense.Id,
                Name = expense.Name,
                Description = expense.Description,
                Amount = expense.Amount,
                Currency = expense.Currency,
                ExpenseDate = expense.ExpenseDate,
                CreatedAt = expense.CreatedAt,
                UpdatedAt = expense.UpdatedAt,
                ActivityId = expense.ActivityId,
                TaskId = expense.TaskId,
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
                    User = new UserDto
                    {
                        Id = es.User.Id,
                        Name = es.User.Name,
                        Email = es.User.Email,
                        CreatedAt = es.User.CreatedAt
                    },
                    ShareAmount = es.ShareAmount,
                    SharePercentage = es.SharePercentage,
                    IsPaid = es.IsPaid,
                    PaidAt = es.PaidAt
                }).ToList()
            };

            return Ok(expenseDto);
        }

        [HttpPost]
        public async Task<ActionResult<ExpenseDto>> CreateExpense(int activityId, CreateExpenseDto createExpenseDto)
        {
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState); // 422
            }

            var userId = GetCurrentUserId();
            
            var activity = await _context.Activities
                .Include(a => a.ActivityUsers)
                .FirstOrDefaultAsync(a => a.Id == activityId && (a.CreatedByUserId == userId || a.ActivityUsers.Any(au => au.UserId == userId)));

            if (activity == null)
            {
                return NotFound("Activity not found or no access");
            }

            // Validate task if provided
            if (createExpenseDto.TaskId.HasValue)
            {
                var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == createExpenseDto.TaskId.Value && t.ActivityId == activityId);
                if (task == null)
                {
                    return BadRequest("Task not found or not part of this activity");
                }
            }

            var expense = new Expense
            {
                Name = createExpenseDto.Name,
                Description = createExpenseDto.Description,
                Amount = createExpenseDto.Amount,
                Currency = createExpenseDto.Currency,
                ExpenseDate = createExpenseDto.ExpenseDate,
                ActivityId = activityId,
                TaskId = createExpenseDto.TaskId,
                PaidByUserId = userId
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            // Create expense shares
            if (createExpenseDto.Shares.Any())
            {
                // Validate that all shares are for participants
                var shareUserIds = createExpenseDto.Shares.Select(s => s.UserId).ToList();
                var participants = await _context.ActivityUsers
                    .Where(au => au.ActivityId == activityId && shareUserIds.Contains(au.UserId))
                    .Select(au => au.UserId)
                    .ToListAsync();

                // Add activity creator if not already in participants
                if (activity.CreatedByUserId != userId && !participants.Contains(activity.CreatedByUserId))
                {
                    participants.Add(activity.CreatedByUserId);
                }

                var invalidUsers = shareUserIds.Except(participants).ToList();
                if (invalidUsers.Any())
                {
                    _context.Expenses.Remove(expense);
                    await _context.SaveChangesAsync();
                    return BadRequest($"Users {string.Join(", ", invalidUsers)} are not participants of this activity");
                }

                // Validate that share percentages add up to 100%
                var totalPercentage = createExpenseDto.Shares.Sum(s => s.SharePercentage);
                if (Math.Abs(totalPercentage - 100) > 0.01m)
                {
                    _context.Expenses.Remove(expense);
                    await _context.SaveChangesAsync();
                    return BadRequest("Share percentages must add up to 100%");
                }

                var expenseShares = createExpenseDto.Shares.Select(s => new ExpenseShare
                {
                    ExpenseId = expense.Id,
                    UserId = s.UserId,
                    ShareAmount = Math.Round(expense.Amount * s.SharePercentage / 100, 2),
                    SharePercentage = s.SharePercentage
                }).ToList();

                _context.ExpenseShares.AddRange(expenseShares);
                await _context.SaveChangesAsync();
            }
            else
            {
                // If no shares provided, split equally among all participants
                var allParticipants = await _context.ActivityUsers
                    .Where(au => au.ActivityId == activityId)
                    .Select(au => au.UserId)
                    .ToListAsync();

                // Add activity creator if not already in participants
                if (!allParticipants.Contains(activity.CreatedByUserId))
                {
                    allParticipants.Add(activity.CreatedByUserId);
                }

                var sharePercentage = Math.Round(100m / allParticipants.Count, 2);
                var shareAmount = Math.Round(expense.Amount / allParticipants.Count, 2);

                var expenseShares = allParticipants.Select(userId => new ExpenseShare
                {
                    ExpenseId = expense.Id,
                    UserId = userId,
                    ShareAmount = shareAmount,
                    SharePercentage = sharePercentage
                }).ToList();

                _context.ExpenseShares.AddRange(expenseShares);
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetExpense), new { activityId, id = expense.Id }, await GetExpenseDto(expense.Id));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExpense(int activityId, int id, UpdateExpenseDto updateExpenseDto)
        {
            var userId = GetCurrentUserId();
            
            var expense = await _context.Expenses
                .Include(e => e.Activity)
                .ThenInclude(a => a.ActivityUsers)
                .FirstOrDefaultAsync(e => e.Id == id && e.ActivityId == activityId);

            if (expense == null)
            {
                return NotFound();
            }

            // Check if user has access to this activity and is the one who paid
            if (expense.Activity.CreatedByUserId != userId && !expense.Activity.ActivityUsers.Any(au => au.UserId == userId))
            {
                return NotFound();
            }

            // Only the person who paid can edit the expense
            if (expense.PaidByUserId != userId)
            {
                return Forbid("Only the person who paid can edit this expense");
            }

            // Validate task if provided
            if (updateExpenseDto.TaskId.HasValue)
            {
                var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == updateExpenseDto.TaskId.Value && t.ActivityId == activityId);
                if (task == null)
                {
                    return BadRequest("Task not found or not part of this activity");
                }
            }

            if (updateExpenseDto.Name != null) expense.Name = updateExpenseDto.Name;
            if (updateExpenseDto.Description != null) expense.Description = updateExpenseDto.Description;
            if (updateExpenseDto.Amount.HasValue) 
            {
                expense.Amount = updateExpenseDto.Amount.Value;
                
                // Update share amounts proportionally
                var shares = await _context.ExpenseShares.Where(es => es.ExpenseId == id).ToListAsync();
                foreach (var share in shares)
                {
                    share.ShareAmount = Math.Round(expense.Amount * share.SharePercentage / 100, 2);
                }
            }
            if (updateExpenseDto.Currency != null) expense.Currency = updateExpenseDto.Currency;
            if (updateExpenseDto.ExpenseDate.HasValue) expense.ExpenseDate = updateExpenseDto.ExpenseDate.Value;
            if (updateExpenseDto.TaskId.HasValue) expense.TaskId = updateExpenseDto.TaskId.Value;
            
            expense.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense(int activityId, int id)
        {
            var userId = GetCurrentUserId();
            
            var expense = await _context.Expenses
                .Include(e => e.Activity)
                .ThenInclude(a => a.ActivityUsers)
                .FirstOrDefaultAsync(e => e.Id == id && e.ActivityId == activityId);

            if (expense == null)
            {
                return NotFound();
            }

            // Check if user has access to this activity
            var isActivityCreator = expense.Activity.CreatedByUserId == userId;
            var isActivityAdmin = expense.Activity.ActivityUsers.Any(au => au.UserId == userId && au.IsAdmin);
            var isPaidBy = expense.PaidByUserId == userId;

            if (!isActivityCreator && !isActivityAdmin && !isPaidBy)
            {
                return Forbid();
            }

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id}/shares/{shareId}/pay")]
        public async Task<IActionResult> MarkShareAsPaid(int activityId, int id, int shareId)
        {
            var userId = GetCurrentUserId();
            
            var expense = await _context.Expenses
                .Include(e => e.Activity)
                .ThenInclude(a => a.ActivityUsers)
                .Include(e => e.ExpenseShares)
                .FirstOrDefaultAsync(e => e.Id == id && e.ActivityId == activityId);

            if (expense == null)
            {
                return NotFound();
            }

            // Check if user has access to this activity
            if (expense.Activity.CreatedByUserId != userId && !expense.Activity.ActivityUsers.Any(au => au.UserId == userId))
            {
                return NotFound();
            }

            var share = expense.ExpenseShares.FirstOrDefault(es => es.Id == shareId);
            if (share == null)
            {
                return NotFound("Share not found");
            }

            // Only the person who paid or the person who owes can mark as paid
            if (expense.PaidByUserId != userId && share.UserId != userId)
            {
                return Forbid("Only the payer or the person who owes can mark this as paid");
            }

            share.IsPaid = true;
            share.PaidAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}/shares/{shareId}/pay")]
        public async Task<IActionResult> MarkShareAsUnpaid(int activityId, int id, int shareId)
        {
            var userId = GetCurrentUserId();
            
            var expense = await _context.Expenses
                .Include(e => e.Activity)
                .ThenInclude(a => a.ActivityUsers)
                .Include(e => e.ExpenseShares)
                .FirstOrDefaultAsync(e => e.Id == id && e.ActivityId == activityId);

            if (expense == null)
            {
                return NotFound();
            }

            // Check if user has access to this activity
            if (expense.Activity.CreatedByUserId != userId && !expense.Activity.ActivityUsers.Any(au => au.UserId == userId))
            {
                return NotFound();
            }

            var share = expense.ExpenseShares.FirstOrDefault(es => es.Id == shareId);
            if (share == null)
            {
                return NotFound("Share not found");
            }

            // Only the person who paid can mark as unpaid
            if (expense.PaidByUserId != userId)
            {
                return Forbid("Only the payer can mark this as unpaid");
            }

            share.IsPaid = false;
            share.PaidAt = null;

            await _context.SaveChangesAsync();

            return Ok();
        }

        private async Task<ExpenseDto?> GetExpenseDto(int id)
        {
            var expense = await _context.Expenses
                .Include(e => e.PaidBy)
                .Include(e => e.ExpenseShares)
                .ThenInclude(es => es.User)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (expense == null) return null;

            return new ExpenseDto
            {
                Id = expense.Id,
                Name = expense.Name,
                Description = expense.Description,
                Amount = expense.Amount,
                Currency = expense.Currency,
                ExpenseDate = expense.ExpenseDate,
                CreatedAt = expense.CreatedAt,
                UpdatedAt = expense.UpdatedAt,
                ActivityId = expense.ActivityId,
                TaskId = expense.TaskId,
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
                    User = new UserDto
                    {
                        Id = es.User.Id,
                        Name = es.User.Name,
                        Email = es.User.Email,
                        CreatedAt = es.User.CreatedAt
                    },
                    ShareAmount = es.ShareAmount,
                    SharePercentage = es.SharePercentage,
                    IsPaid = es.IsPaid,
                    PaidAt = es.PaidAt
                }).ToList()
            };
        }
    }
}