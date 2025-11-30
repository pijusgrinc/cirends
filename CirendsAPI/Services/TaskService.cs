using CirendsAPI.Data;
using CirendsAPI.DTOs;
using CirendsAPI.Exceptions;
using CirendsAPI.Models;
using Microsoft.EntityFrameworkCore;
using UnauthorizedAccessException = CirendsAPI.Exceptions.UnauthorizedAccessException;

namespace CirendsAPI.Services
{
    public interface ITaskService
    {
        Task<TaskItem?> GetTaskAsync(int activityId, int taskId, int userId);

        Task<IEnumerable<TaskItem>> GetTasksAsync(int activityId, int userId);

        Task<TaskItem> CreateTaskAsync(int activityId, int userId, CreateTaskDto createTaskDto);

        Task UpdateTaskAsync(int activityId, int taskId, int userId, UpdateTaskDto updateTaskDto);

        Task DeleteTaskAsync(int activityId, int taskId, int userId);
    }

    public class TaskService : ITaskService
    {
        private readonly CirendsDbContext _context;

        public TaskService(CirendsDbContext context)
        {
            _context = context;
        }

        public async Task<TaskItem?> GetTaskAsync(int activityId, int taskId, int userId)
        {
            var activity = await _context.Activities
                .Include(a => a.ActivityUsers)
                .FirstOrDefaultAsync(a => a.Id == activityId);

            if (activity == null)
            {
                throw new NotFoundException("Activity not found");
            }

            var hasAccess = activity.CreatedByUserId == userId ||
                        activity.ActivityUsers.Any(au => au.UserId == userId);

            if (!hasAccess)
            {
                throw new UnauthorizedAccessException("No access to this activity");
            }

            var task = await _context.Tasks
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .Include(t => t.Expenses)
                .ThenInclude(e => e.PaidBy)
                .Include(t => t.Expenses)
                .ThenInclude(e => e.ExpenseShares)
                .ThenInclude(es => es.User)
                .FirstOrDefaultAsync(t => t.Id == taskId && t.ActivityId == activityId);

            return task;
        }

        public async Task<IEnumerable<TaskItem>> GetTasksAsync(int activityId, int userId)
        {
            var hasAccess = await _context.Activities
                .AnyAsync(a => a.Id == activityId &&
                    (a.CreatedByUserId == userId || a.ActivityUsers.Any(au => au.UserId == userId)));

            if (!hasAccess)
                throw new UnauthorizedAccessException("Activity not found or no access");

            return await _context.Tasks
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .Include(t => t.Expenses)
                .ThenInclude(e => e.PaidBy)
                .Where(t => t.ActivityId == activityId)
                .ToListAsync();
        }

        public async Task<TaskItem> CreateTaskAsync(int activityId, int userId, CreateTaskDto createTaskDto)
        {
            var activity = await _context.Activities
                .Include(a => a.ActivityUsers)
                .FirstOrDefaultAsync(a => a.Id == activityId &&
                    (a.CreatedByUserId == userId || a.ActivityUsers.Any(au => au.UserId == userId)));

            if (activity == null)
                throw new UnauthorizedAccessException("Activity not found or no access");

            if (createTaskDto.AssignedToUserId.HasValue)
            {
                var assignedUser = await _context.Users.FindAsync(createTaskDto.AssignedToUserId.Value);
                if (assignedUser == null)
                    throw new ArgumentException("Assigned user not found");

                var isParticipant = activity.CreatedByUserId == createTaskDto.AssignedToUserId.Value ||
                                  activity.ActivityUsers.Any(au => au.UserId == createTaskDto.AssignedToUserId.Value);

                if (!isParticipant)
                    throw new ArgumentException("Assigned user is not a participant of this activity");
            }

            var task = new TaskItem
            {
                Name = createTaskDto.Name,
                Description = createTaskDto.Description,
                DueDate = createTaskDto.DueDate,
                Priority = createTaskDto.Priority,
                ActivityId = activityId,
                AssignedToUserId = createTaskDto.AssignedToUserId,
                CreatedByUserId = userId
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return task;
        }

        public async Task UpdateTaskAsync(int activityId, int taskId, int userId, UpdateTaskDto updateTaskDto)
        {
            var task = await _context.Tasks
                .Include(t => t.Activity)
                .ThenInclude(a => a.ActivityUsers)
                .FirstOrDefaultAsync(t => t.Id == taskId && t.ActivityId == activityId);

            if (task == null)
                throw new NotFoundException("Task not found");

            var hasAccess = task.Activity.CreatedByUserId == userId ||
                           task.Activity.ActivityUsers.Any(au => au.UserId == userId);

            if (!hasAccess)
                throw new UnauthorizedAccessException("No access to this task");

            if (updateTaskDto.AssignedToUserId.HasValue)
            {
                var assignedUser = await _context.Users.FindAsync(updateTaskDto.AssignedToUserId.Value);
                if (assignedUser == null)
                    throw new ArgumentException("Assigned user not found");

                var isParticipant = task.Activity.CreatedByUserId == updateTaskDto.AssignedToUserId.Value ||
                                  task.Activity.ActivityUsers.Any(au => au.UserId == updateTaskDto.AssignedToUserId.Value);

                if (!isParticipant)
                    throw new ArgumentException("Assigned user is not a participant of this activity");
            }

            // Apply updates
            if (updateTaskDto.Name != null) task.Name = updateTaskDto.Name;
            if (updateTaskDto.Description != null) task.Description = updateTaskDto.Description;
            if (updateTaskDto.DueDate.HasValue) task.DueDate = updateTaskDto.DueDate;
            if (updateTaskDto.Status.HasValue)
            {
                task.Status = updateTaskDto.Status.Value;
                if (updateTaskDto.Status.Value == TaskItemStatus.Completed && task.CompletedAt == null)
                {
                    task.CompletedAt = DateTime.UtcNow;
                }
                else if (updateTaskDto.Status.Value != TaskItemStatus.Completed)
                {
                    task.CompletedAt = null;
                }
            }
            if (updateTaskDto.Priority.HasValue) task.Priority = updateTaskDto.Priority.Value;
            if (updateTaskDto.AssignedToUserId.HasValue) task.AssignedToUserId = updateTaskDto.AssignedToUserId.Value;

            task.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTaskAsync(int activityId, int taskId, int userId)
        {
            var task = await _context.Tasks
                .Include(t => t.Activity)
                .ThenInclude(a => a.ActivityUsers)
                .FirstOrDefaultAsync(t => t.Id == taskId && t.ActivityId == activityId);

            if (task == null)
                throw new NotFoundException("Task not found");

            var isCreator = task.CreatedByUserId == userId;
            var isActivityCreator = task.Activity.CreatedByUserId == userId;
            var isActivityAdmin = task.Activity.ActivityUsers.Any(au => au.UserId == userId && au.IsAdmin);

            if (!isCreator && !isActivityCreator && !isActivityAdmin)
                throw new UnauthorizedAccessException("No permission to delete this task");

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
        }
    }
}