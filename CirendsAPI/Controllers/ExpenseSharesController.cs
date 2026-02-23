using CirendsAPI.Data;
using CirendsAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CirendsAPI.Controllers
{
    [ApiController]
    [Route("api/activities/{activityId}/expenses/{expenseId}/shares")]
    [Authorize]
    public class ExpenseSharesController : ControllerBase
    {
        private readonly CirendsDbContext _context;
        private readonly ILogger<ExpenseSharesController> _logger;

        public ExpenseSharesController(CirendsDbContext context, ILogger<ExpenseSharesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Mark expense share as paid
        /// </summary>
        [HttpPatch("{shareId}/mark-paid")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> MarkShareAsPaid(
            int activityId,
            int expenseId,
            int shareId)
        {
            var validation = ValidationHelper.ValidateHierarchyIds(
                (activityId, "activityId"),
                (expenseId, "expenseId"),
                (shareId, "shareId"));
            
            if (validation != null) return validation;

            if (!ValidationHelper.TryGetCurrentUserId(User, out var userId))
            {
                return ValidationHelper.InvalidAuthenticationResponse();
            }

            try
            {
                // Verify hierarchy
                var expense = await _context.Expenses
                    .FirstOrDefaultAsync(e => e.Id == expenseId && e.ActivityId == activityId);

                if (expense == null)
                {
                    return NotFound(new { message = "Expense not found or hierarchy mismatch", error = "EXPENSE_NOT_FOUND" });
                }

                var share = await _context.ExpenseShares
                    .FirstOrDefaultAsync(s => s.Id == shareId && s.ExpenseId == expenseId);

                if (share == null)
                {
                    return NotFound(new { message = "Expense share not found", error = "SHARE_NOT_FOUND" });
                }

                // Only the user who owes can mark as paid, or the one who paid can confirm
                if (share.UserId != userId && expense.PaidByUserId != userId)
                {
                    return Forbid();
                }

                share.IsPaid = true;
                share.PaidAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking share {ShareId} as paid", shareId);
                return StatusCode(500, new { message = "Internal server error", error = "DATABASE_ERROR" });
            }
        }

        /// <summary>
        /// Unmark expense share as paid (undo)
        /// </summary>
        [HttpPatch("{shareId}/unmark-paid")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UnmarkShareAsPaid(
            int activityId,
            int expenseId,
            int shareId)
        {
            var validation = ValidationHelper.ValidateHierarchyIds(
                (activityId, "activityId"),
                (expenseId, "expenseId"),
                (shareId, "shareId"));
            
            if (validation != null) return validation;

            if (!ValidationHelper.TryGetCurrentUserId(User, out var userId))
            {
                return ValidationHelper.InvalidAuthenticationResponse();
            }

            try
            {
                // Verify hierarchy
                var expense = await _context.Expenses
                    .FirstOrDefaultAsync(e => e.Id == expenseId && e.ActivityId == activityId);

                if (expense == null)
                {
                    return NotFound(new { message = "Expense not found or hierarchy mismatch", error = "EXPENSE_NOT_FOUND" });
                }

                var share = await _context.ExpenseShares
                    .FirstOrDefaultAsync(s => s.Id == shareId && s.ExpenseId == expenseId);

                if (share == null)
                {
                    return NotFound(new { message = "Expense share not found", error = "SHARE_NOT_FOUND" });
                }

                // Allow the debtor (share owner) or payer to unmark
                if (expense.PaidByUserId != userId && share.UserId != userId)
                {
                    return Forbid();
                }

                share.IsPaid = false;
                share.PaidAt = null;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unmarking share {ShareId} as paid", shareId);
                return StatusCode(500, new { message = "Internal server error", error = "DATABASE_ERROR" });
            }
        }

        /// <summary>
        /// Mark all expense shares for an activity as paid
        /// </summary>
        [HttpPost("/api/activities/{activityId}/expenses/mark-all-paid")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> MarkAllExpensesAsPaidForActivity(
            int activityId)
        {
            var validation = ValidationHelper.ValidatePositiveId(activityId, "activityId");
            if (validation != null) return validation;

            if (!ValidationHelper.TryGetCurrentUserId(User, out var userId))
            {
                return ValidationHelper.InvalidAuthenticationResponse();
            }

            try
            {
                var activity = await _context.Activities
                    .Include(a => a.ActivityUsers)
                    .Include(a => a.Expenses)
                        .ThenInclude(e => e.ExpenseShares)
                    .FirstOrDefaultAsync(a => a.Id == activityId);

                if (activity == null)
                {
                    return NotFound(new { message = "Activity not found", error = "ACTIVITY_NOT_FOUND" });
                }

                // Check authorization - creator, participant, or admin
                var isCreator = activity.CreatedByUserId == userId;
                var isActivityMember = activity.ActivityUsers.Any(au => au.UserId == userId);
                var isAdmin = User.IsInRole("Admin");

                if (!isCreator && !isActivityMember && !isAdmin)
                {
                    return StatusCode(403, new { message = "You don't have permission to mark expenses as paid", error = "FORBIDDEN" });
                }

                // Mark all expense shares as paid
                var now = DateTime.UtcNow;
                foreach (var expense in activity.Expenses)
                {
                    foreach (var share in expense.ExpenseShares)
                    {
                        if (!share.IsPaid)
                        {
                            share.IsPaid = true;
                            share.PaidAt = now;
                        }
                    }
                }

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all expenses as paid for activity {ActivityId}", activityId);
                return StatusCode(500, new { message = "Internal server error", error = "DATABASE_ERROR" });
            }
        }
    }
}
