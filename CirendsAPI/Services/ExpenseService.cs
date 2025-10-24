using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CirendsAPI.Data;
using CirendsAPI.DTOs;
using CirendsAPI.Models;
using CirendsAPI.Exceptions;

namespace CirendsAPI.Services
{
    public interface IExpenseService
    {
        Task<IEnumerable<ExpenseDto>> GetExpensesAsync(int activityId, int taskId, int userId);
        Task<ExpenseDto> GetExpenseAsync(int activityId, int taskId, int expenseId, int userId);
        Task<ExpenseDto> CreateExpenseAsync(int activityId, int taskId, int userId, CreateExpenseDto createExpenseDto);
        Task UpdateExpenseAsync(int activityId, int taskId, int expenseId, int userId, UpdateExpenseDto updateExpenseDto);
        Task DeleteExpenseAsync(int activityId, int taskId, int expenseId, int userId);
    }

    public class ExpenseService : IExpenseService
    {
        private readonly CirendsDbContext _context;
        private readonly ITaskService _taskService;
        private readonly IMapper _mapper;

        public ExpenseService(CirendsDbContext context, ITaskService taskService, IMapper mapper)
        {
            _context = context;
            _taskService = taskService;
            _mapper = mapper;
        }

        public async Task<ExpenseDto> GetExpenseAsync(int activityId, int taskId, int expenseId, int userId)
        {
            var task = await _taskService.GetTaskAsync(activityId, taskId, userId);
            if (task == null)
                throw new NotFoundException("Task not found");

            var expense = await _context.Expenses
                .Include(e => e.PaidBy)
                .Include(e => e.ExpenseShares)
                .ThenInclude(es => es.User)
                .FirstOrDefaultAsync(e => e.Id == expenseId && e.TaskId == taskId);

            if (expense == null)
                throw new NotFoundException("Expense not found");

            return _mapper.Map<ExpenseDto>(expense);
        }
        public async Task<IEnumerable<ExpenseDto>> GetExpensesAsync(int activityId, int taskId, int userId)
        {
            var task = await _context.Tasks
                .Include(t => t.Activity)
                .FirstOrDefaultAsync(t => t.Id == taskId && t.ActivityId == activityId);

            if (task == null)
                throw new NotFoundException("Task not found");

            var hasAccess = task.Activity.CreatedByUserId == userId ||
                            task.Activity.ActivityUsers.Any(au => au.UserId == userId);

            if (!hasAccess)
                throw new CirendsAPI.Exceptions.UnauthorizedAccessException("No access to this activity");

            var expenses = await _context.Expenses
                .Where(e => e.TaskId == taskId)
                .Include(e => e.PaidBy)
                .Include(e => e.ExpenseShares)
                .ThenInclude(es => es.User)
                .ToListAsync();

            return expenses.Select(e => _mapper.Map<ExpenseDto>(e));
        }

        public async Task<ExpenseDto> CreateExpenseAsync(int activityId, int taskId, int userId, CreateExpenseDto createExpenseDto)
        {
            var task = await _context.Tasks
                .Include(t => t.Activity)
                .FirstOrDefaultAsync(t => t.Id == taskId && t.ActivityId == activityId);

            if (task == null)
                throw new NotFoundException("Task not found");

            var hasAccess = task.Activity.CreatedByUserId == userId ||
                            task.Activity.ActivityUsers.Any(au => au.UserId == userId);

            if (!hasAccess)
                throw new CirendsAPI.Exceptions.UnauthorizedAccessException("No access to this activity");

            var expense = new Expense
            {
                Name = createExpenseDto.Name,
                Description = createExpenseDto.Description,
                Amount = createExpenseDto.Amount,
                Currency = createExpenseDto.Currency,
                ExpenseDate = createExpenseDto.ExpenseDate,
                TaskId = taskId,
                PaidByUserId = userId
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();
            
            var createdExpense = await _context.Expenses
            .Include(e => e.PaidBy)
            .Include(e => e.ExpenseShares)
            .ThenInclude(es => es.User)
            .FirstOrDefaultAsync(e => e.Id == expense.Id);

            return _mapper.Map<ExpenseDto>(createdExpense);
        }

        public async Task UpdateExpenseAsync(int activityId, int taskId, int expenseId, int userId, UpdateExpenseDto updateExpenseDto)
        {
            var expense = await GetExpenseForUpdate(activityId, taskId, expenseId, userId);
            _mapper.Map(updateExpenseDto, expense);
            expense.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteExpenseAsync(int activityId, int taskId, int expenseId, int userId)
        {
            var expense = await GetExpenseForUpdate(activityId, taskId, expenseId, userId);
            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();
        }

        private async Task<Expense> GetExpenseForUpdate(int activityId, int taskId, int expenseId, int userId)
        {
            var task = await _taskService.GetTaskAsync(activityId, taskId, userId);
            if (task == null)
                throw new NotFoundException("Task not found");

            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == expenseId && e.TaskId == taskId);

            if (expense == null)
                throw new NotFoundException("Expense not found");

            if (expense.PaidByUserId != userId)
                throw new CirendsAPI.Exceptions.UnauthorizedAccessException("Only expense payer can modify this expense");

            return expense;
        }
    }
}