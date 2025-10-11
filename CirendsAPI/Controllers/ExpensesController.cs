using AutoMapper;
using CirendsAPI.DTOs;
using UnauthorizedAccessException = CirendsAPI.Exceptions.UnauthorizedAccessException2;
using CirendsAPI.Exceptions;
using CirendsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CirendsAPI.Controllers
{
    [ApiController]
    [Route("api/activities/{activityId}/tasks/{taskId}/expenses")]
    [Authorize]
    public class ExpensesController : ControllerBase
    {
        private readonly IExpenseService _expenseService;
        private readonly IMapper _mapper;

        public ExpensesController(IExpenseService expenseService, IMapper mapper)
        {
            _expenseService = expenseService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExpenseDto>>> GetExpenses(int activityId, int taskId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var expenses = await _expenseService.GetExpensesAsync(activityId, taskId, userId);
                return Ok(expenses);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message); // 404 klaida
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message); // 403 klaida
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ExpenseDto>> GetExpense(int activityId, int taskId, int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var expense = await _expenseService.GetExpenseAsync(activityId, taskId, id, userId);
                
                if (expense == null)
                    return NotFound();

                return Ok(_mapper.Map<ExpenseDto>(expense));
            }
            catch (UnauthorizedAccessException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateExpense(int activityId, int taskId, CreateExpenseDto createExpenseDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var expense = await _expenseService.CreateExpenseAsync(activityId, taskId, userId, createExpenseDto);
                return CreatedAtAction(nameof(GetExpenses), new { activityId, taskId }, expense); // 201 klaida
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message); // 404 klaida
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message); // 403 klaida
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message); // 400 klaida
            }
        }

        [HttpPut("{expenseId}")]
        public async Task<IActionResult> UpdateExpense(int activityId, int taskId, int expenseId, [FromBody] UpdateExpenseDto updateExpenseDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _expenseService.UpdateExpenseAsync(activityId, taskId, expenseId, userId, updateExpenseDto);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (CirendsAPI.Exceptions.UnauthorizedAccessException2 ex)
            {
                return Forbid(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{expenseId}")]
        public async Task<IActionResult> DeleteExpense(int activityId, int taskId, int expenseId)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _expenseService.DeleteExpenseAsync(activityId, taskId, expenseId, userId);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (CirendsAPI.Exceptions.UnauthorizedAccessException2 ex)
            {
                return Forbid(ex.Message);
            }
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }
    }
}