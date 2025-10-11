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
    public class TasksController : ControllerBase
    {
        private readonly CirendsDbContext _context;

        public TasksController(CirendsDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetTasks(int activityId)
        {
            var userId = GetCurrentUserId();
            
            // Check if user has access to this activity
            var hasAccess = await _context.Activities
                .AnyAsync(a => a.Id == activityId && (a.CreatedByUserId == userId || a.ActivityUsers.Any(au => au.UserId == userId)));

            if (!hasAccess)
            {
                return NotFound("Activity not found or no access");
            }

            var tasks = await _context.Tasks
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .Include(t => t.Expenses)
                .ThenInclude(e => e.PaidBy)
                .Where(t => t.ActivityId == activityId)
                .ToListAsync();

            var taskDtos = tasks.Select(t => new TaskItemDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                DueDate = t.DueDate,
                Status = t.Status,
                Priority = t.Priority,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                CompletedAt = t.CompletedAt,
                ActivityId = t.ActivityId,
                AssignedTo = t.AssignedTo == null ? null : new UserDto
                {
                    Id = t.AssignedTo.Id,
                    Name = t.AssignedTo.Name,
                    Email = t.AssignedTo.Email,
                    CreatedAt = t.AssignedTo.CreatedAt
                },
                CreatedBy = new UserDto
                {
                    Id = t.CreatedBy.Id,
                    Name = t.CreatedBy.Name,
                    Email = t.CreatedBy.Email,
                    CreatedAt = t.CreatedBy.CreatedAt
                },
                Expenses = t.Expenses.Select(e => new ExpenseDto
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
                    }
                }).ToList()
            }).ToList();

            return Ok(taskDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItemDto>> GetTask(int activityId, int id)
        {
            var userId = GetCurrentUserId();
            
            var task = await _context.Tasks
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .Include(t => t.Activity)
                .ThenInclude(a => a.ActivityUsers)
                .Include(t => t.Expenses)
                .ThenInclude(e => e.PaidBy)
                .Where(t => t.Id == id && t.ActivityId == activityId)
                .FirstOrDefaultAsync();

            if (task == null)
            {
                return NotFound();
            }

            // Check if user has access to this activity
            if (task.Activity.CreatedByUserId != userId && !task.Activity.ActivityUsers.Any(au => au.UserId == userId))
            {
                return NotFound();
            }

            var taskDto = new TaskItemDto
            {
                Id = task.Id,
                Name = task.Name,
                Description = task.Description,
                DueDate = task.DueDate,
                Status = task.Status,
                Priority = task.Priority,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                CompletedAt = task.CompletedAt,
                ActivityId = task.ActivityId,
                AssignedTo = task.AssignedTo == null ? null : new UserDto
                {
                    Id = task.AssignedTo.Id,
                    Name = task.AssignedTo.Name,
                    Email = task.AssignedTo.Email,
                    CreatedAt = task.AssignedTo.CreatedAt
                },
                CreatedBy = new UserDto
                {
                    Id = task.CreatedBy.Id,
                    Name = task.CreatedBy.Name,
                    Email = task.CreatedBy.Email,
                    CreatedAt = task.CreatedBy.CreatedAt
                },
                Expenses = task.Expenses.Select(e => new ExpenseDto
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
                    }
                }).ToList()
            };

            return Ok(taskDto);
        }

        [HttpPost]
        public async Task<ActionResult<TaskItemDto>> CreateTask(int activityId, CreateTaskDto createTaskDto)
        {
            var userId = GetCurrentUserId();
            
            // Check if user has access to this activity
            var activity = await _context.Activities
                .Include(a => a.ActivityUsers)
                .FirstOrDefaultAsync(a => a.Id == activityId && (a.CreatedByUserId == userId || a.ActivityUsers.Any(au => au.UserId == userId)));

            if (activity == null)
            {
                return NotFound("Activity not found or no access");
            }

            // Validate assigned user if provided
            if (createTaskDto.AssignedToUserId.HasValue)
            {
                var assignedUser = await _context.Users.FindAsync(createTaskDto.AssignedToUserId.Value);
                if (assignedUser == null)
                {
                    return BadRequest("Assigned user not found");
                }

                // Check if assigned user is participant of the activity
                var isParticipant = activity.CreatedByUserId == createTaskDto.AssignedToUserId.Value ||
                                  activity.ActivityUsers.Any(au => au.UserId == createTaskDto.AssignedToUserId.Value);
                
                if (!isParticipant)
                {
                    return BadRequest("Assigned user is not a participant of this activity");
                }
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

            return CreatedAtAction(nameof(GetTask), new { activityId, id = task.Id }, await GetTaskDto(task.Id));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int activityId, int id, UpdateTaskDto updateTaskDto)
        {
            var userId = GetCurrentUserId();
            
            var task = await _context.Tasks
                .Include(t => t.Activity)
                .ThenInclude(a => a.ActivityUsers)
                .FirstOrDefaultAsync(t => t.Id == id && t.ActivityId == activityId);

            if (task == null)
            {
                return NotFound();
            }

            // Check if user has access to this activity
            if (task.Activity.CreatedByUserId != userId && !task.Activity.ActivityUsers.Any(au => au.UserId == userId))
            {
                return NotFound();
            }

            // Validate assigned user if provided
            if (updateTaskDto.AssignedToUserId.HasValue)
            {
                var assignedUser = await _context.Users.FindAsync(updateTaskDto.AssignedToUserId.Value);
                if (assignedUser == null)
                {
                    return BadRequest("Assigned user not found");
                }

                // Check if assigned user is participant of the activity
                var isParticipant = task.Activity.CreatedByUserId == updateTaskDto.AssignedToUserId.Value ||
                                  task.Activity.ActivityUsers.Any(au => au.UserId == updateTaskDto.AssignedToUserId.Value);
                
                if (!isParticipant)
                {
                    return BadRequest("Assigned user is not a participant of this activity");
                }
            }

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

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int activityId, int id)
        {
            var userId = GetCurrentUserId();
            
            var task = await _context.Tasks
                .Include(t => t.Activity)
                .ThenInclude(a => a.ActivityUsers)
                .FirstOrDefaultAsync(t => t.Id == id && t.ActivityId == activityId);

            if (task == null)
            {
                return NotFound();
            }

            // Check if user has access to this activity and is creator or admin
            var isCreator = task.CreatedByUserId == userId;
            var isActivityCreator = task.Activity.CreatedByUserId == userId;
            var isActivityAdmin = task.Activity.ActivityUsers.Any(au => au.UserId == userId && au.IsAdmin);

            if (!isCreator && !isActivityCreator && !isActivityAdmin)
            {
                return Forbid();
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<TaskItemDto?> GetTaskDto(int id)
        {
            var task = await _context.Tasks
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return null;

            return new TaskItemDto
            {
                Id = task.Id,
                Name = task.Name,
                Description = task.Description,
                DueDate = task.DueDate,
                Status = task.Status,
                Priority = task.Priority,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                CompletedAt = task.CompletedAt,
                ActivityId = task.ActivityId,
                AssignedTo = task.AssignedTo == null ? null : new UserDto
                {
                    Id = task.AssignedTo.Id,
                    Name = task.AssignedTo.Name,
                    Email = task.AssignedTo.Email,
                    CreatedAt = task.AssignedTo.CreatedAt
                },
                CreatedBy = new UserDto
                {
                    Id = task.CreatedBy.Id,
                    Name = task.CreatedBy.Name,
                    Email = task.CreatedBy.Email,
                    CreatedAt = task.CreatedBy.CreatedAt
                }
            };
        }
    }
}