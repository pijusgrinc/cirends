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
    [Route("api/activities/{activityId}/tasks")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly CirendsDbContext _context;
        private readonly ILogger<TasksController> _logger;

        public TasksController(CirendsDbContext context, ILogger<TasksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all tasks for an activity (hierarchical)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<TaskDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<List<TaskDto>>> GetTasksByActivity(int activityId)
        {
            var validation = ValidationHelper.ValidatePositiveId(activityId, "activityId");
            if (validation != null) return validation;

            try
            {
                var activity = await _context.Activities.FindAsync(activityId);
                if (activity == null)
                {
                    return NotFound(new { message = "Activity not found", error = "ACTIVITY_NOT_FOUND" });
                }

                var tasks = await _context.Tasks
                    .AsNoTracking()
                    .Where(t => t.ActivityId == activityId)
                    .Select(t => new TaskDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Description = t.Description,
                        DueDate = t.DueDate,
                        Priority = (int)t.Priority,
                        Status = (int)t.Status,
                        ActivityId = t.ActivityId,
                        CreatedAt = t.CreatedAt
                    })
                    .ToListAsync();

                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks for activity {ActivityId}", activityId);
                return StatusCode(500, new { message = "Internal server error", error = "DATABASE_ERROR" });
            }
        }

        /// <summary>
        /// Get task by ID (hierarchical: activity -> task)
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TaskDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<TaskDto>> GetTask(int activityId, int id)
        {
            var validation = ValidationHelper.ValidateHierarchyIds((activityId, "activityId"), (id, "taskId"));
            if (validation != null) return validation;

            try
            {
                var activity = await _context.Activities.FindAsync(activityId);
                if (activity == null)
                {
                    return NotFound(new { message = "Activity not found", error = "ACTIVITY_NOT_FOUND" });
                }

                var task = await _context.Tasks
                    .AsNoTracking()
                    .Include(t => t.Expenses)
                    .Include(t => t.CreatedBy)
                    .Include(t => t.AssignedTo)
                    .FirstOrDefaultAsync(t => t.Id == id && t.ActivityId == activityId);

                if (task == null)
                {
                    return NotFound(new { message = "Task not found or does not belong to this activity", error = "TASK_NOT_FOUND" });
                }

                return Ok(new TaskItemDto
                {
                    Id = task.Id,
                    Name = task.Name,
                    Description = task.Description,
                    DueDate = task.DueDate,
                    Priority = task.Priority,
                    Status = task.Status,
                    ActivityId = task.ActivityId,
                    CreatedAt = task.CreatedAt,
                    UpdatedAt = task.UpdatedAt,
                    CompletedAt = task.CompletedAt,
                    AssignedTo = task.AssignedTo != null ? new UserDto
                    {
                        Id = task.AssignedTo.Id,
                        Name = task.AssignedTo.Name,
                        Email = task.AssignedTo.Email,
                        Role = task.AssignedTo.Role,
                        CreatedAt = task.AssignedTo.CreatedAt,
                        UpdatedAt = task.AssignedTo.UpdatedAt,
                        IsActive = task.AssignedTo.IsActive
                    } : null,
                    CreatedBy = new UserDto
                    {
                        Id = task.CreatedBy.Id,
                        Name = task.CreatedBy.Name,
                        Email = task.CreatedBy.Email,
                        Role = task.CreatedBy.Role,
                        CreatedAt = task.CreatedBy.CreatedAt,
                        UpdatedAt = task.CreatedBy.UpdatedAt,
                        IsActive = task.CreatedBy.IsActive
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
                        TaskId = e.TaskId,
                        PaidBy = e.PaidBy != null ? new UserDto
                        {
                            Id = e.PaidBy.Id,
                            Name = e.PaidBy.Name,
                            Email = e.PaidBy.Email,
                            Role = e.PaidBy.Role,
                            CreatedAt = e.PaidBy.CreatedAt,
                            UpdatedAt = e.PaidBy.UpdatedAt,
                            IsActive = e.PaidBy.IsActive
                        } : null,
                        ExpenseShares = new List<ExpenseShareDto>()
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task {TaskId} for activity {ActivityId}", id, activityId);
                return StatusCode(500, new { message = "Internal server error", error = "DATABASE_ERROR" });
            }
        }

        /// <summary>
        /// Create task (hierarchical)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(TaskDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<TaskDto>> CreateTask(int activityId, [FromBody] CreateTaskDto? createDto)
        {
            var validation = ValidationHelper.ValidatePositiveId(activityId, "activityId");
            if (validation != null) return validation;

            if (createDto == null)
            {
                return BadRequest(new { message = "Request body is required", error = "EMPTY_BODY" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid input data", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
            }

            try
            {
                var activity = await _context.Activities.FindAsync(activityId);
                if (activity == null)
                {
                    return NotFound(new { message = "Activity not found", error = "ACTIVITY_NOT_FOUND" });
                }

                var task = new TaskItem
                {
                    Name = createDto.Name.Trim(),
                    Description = createDto.Description?.Trim(),
                    DueDate = DateTime.SpecifyKind(createDto.DueDate, DateTimeKind.Utc),
                    Priority = (TaskItemPriority)createDto.Priority,
                    Status = TaskItemStatus.Pending,
                    ActivityId = activityId,
                    CreatedByUserId = ValidationHelper.TryGetCurrentUserId(User, out var userId) ? userId : 0,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Tasks.Add(task);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetTask), new { activityId, id = task.Id }, new TaskDto
                {
                    Id = task.Id,
                    Name = task.Name,
                    Description = task.Description,
                    DueDate = task.DueDate,
                    Priority = (int)task.Priority,
                    Status = (int)task.Status,
                    ActivityId = task.ActivityId,
                    CreatedAt = task.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task for activity {ActivityId}", activityId);
                return StatusCode(500, new { message = "Internal server error", error = "DATABASE_ERROR" });
            }
        }

        /// <summary>
        /// Update task
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateTask(int activityId, int id, [FromBody] UpdateTaskDto? updateDto)
        {
            var validation = ValidationHelper.ValidateHierarchyIds((activityId, "activityId"), (id, "taskId"));
            if (validation != null) return validation;

            if (updateDto == null)
            {
                return BadRequest(new { message = "Request body is required", error = "EMPTY_BODY" });
            }

            try
            {
                var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.ActivityId == activityId);
                if (task == null)
                {
                    return NotFound(new { message = "Task not found or does not belong to this activity", error = "TASK_NOT_FOUND" });
                }

                if (!string.IsNullOrWhiteSpace(updateDto.Name))
                    task.Name = updateDto.Name.Trim();
                if (!string.IsNullOrWhiteSpace(updateDto.Description))
                    task.Description = updateDto.Description.Trim();
                if (updateDto.DueDate.HasValue)
                    task.DueDate = DateTime.SpecifyKind(updateDto.DueDate.Value, DateTimeKind.Utc);
                if (updateDto.Priority.HasValue)
                    task.Priority = (TaskItemPriority)updateDto.Priority.Value;
                if (updateDto.Status.HasValue)
                    task.Status = (TaskItemStatus)updateDto.Status.Value;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task {TaskId}", id);
                return StatusCode(500, new { message = "Internal server error", error = "DATABASE_ERROR" });
            }
        }

        /// <summary>
        /// Delete task
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteTask(int activityId, int id)
        {
            var validation = ValidationHelper.ValidateHierarchyIds((activityId, "activityId"), (id, "taskId"));
            if (validation != null) return validation;

            try
            {
                var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.ActivityId == activityId);
                if (task == null)
                {
                    return NotFound(new { message = "Task not found or does not belong to this activity", error = "TASK_NOT_FOUND" });
                }

                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task {TaskId}", id);
                return StatusCode(500, new { message = "Internal server error", error = "DATABASE_ERROR" });
            }
        }
    }

    public class CreateTaskDto
    {
        [Required(ErrorMessage = "Task name is required")]
        [StringLength(200, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Due date is required")]
        public DateTime DueDate { get; set; }

        [Range(0, 3)]
        public int Priority { get; set; } = 1;
    }

    public class UpdateTaskDto
    {
        [StringLength(200, MinimumLength = 2)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }

        [Range(0, 3)]
        public int? Priority { get; set; }

        [Range(0, 2)]
        public int? Status { get; set; }
    }
}