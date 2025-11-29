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
    [Route("api/[controller]")]
    [Authorize]
    public class ActivitiesController : ControllerBase
    {
        private readonly CirendsDbContext _context;
        private readonly ILogger<ActivitiesController> _logger;

        public ActivitiesController(CirendsDbContext context, ILogger<ActivitiesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all activities for current user
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<ActivityDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<List<ActivityDto>>> GetActivities()
        {
            if (!ValidationHelper.TryGetCurrentUserId(User, out var userId))
            {
                return ValidationHelper.InvalidAuthenticationResponse();
            }

            try
            {
                var activities = await _context.Activities
                    .AsNoTracking()
                    .Include(a => a.ActivityUsers)
                    .Include(a => a.CreatedBy)
                    .Where(a => a.CreatedByUserId == userId ||
                           _context.ActivityUsers.Any(au => au.ActivityId == a.Id && au.UserId == userId))
                    .Select(a => new ActivityDto
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Description = a.Description,
                        StartDate = a.StartDate,
                        EndDate = a.EndDate,
                        Location = a.Location,
                        CreatedAt = a.CreatedAt,
                        CreatedBy = a.CreatedBy != null ? new UserDto
                        {
                            Id = a.CreatedBy.Id,
                            Name = a.CreatedBy.Name,
                            Email = a.CreatedBy.Email,
                            Role = a.CreatedBy.Role,
                            CreatedAt = a.CreatedBy.CreatedAt,
                            UpdatedAt = a.CreatedBy.UpdatedAt,
                            IsActive = a.CreatedBy.IsActive
                        } : null,
                        Participants = a.ActivityUsers.Select(au => new ActivityUserDto
                        {
                            ActivityId = au.ActivityId,
                            UserId = au.UserId,
                            IsAdmin = au.IsAdmin,
                            JoinedAt = au.JoinedAt,
                            User = null
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(activities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activities for user {UserId}", userId);
                return StatusCode(500, new { message = "Internal server error", error = "DATABASE_ERROR" });
            }
        }

        /// <summary>
        /// Get activity by ID
        /// </summary>
        /// <param name="id">Activity ID (must be positive)</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ActivityDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<ActivityDto>> GetActivity(int id)
        {
            var validation = ValidationHelper.ValidatePositiveId(id, "activityId");
            if (validation != null) return validation;

            if (!ValidationHelper.TryGetCurrentUserId(User, out var userId))
            {
                return ValidationHelper.InvalidAuthenticationResponse();
            }

            try
            {
                var activity = await _context.Activities
                    .AsNoTracking()
                    .Include(a => a.CreatedBy)
                    .Include(a => a.Tasks)
                        .ThenInclude(t => t.CreatedBy)
                    .Include(a => a.Tasks)
                        .ThenInclude(t => t.AssignedTo)
                    .Include(a => a.ActivityUsers)
                        .ThenInclude(au => au.User)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (activity == null)
                {
                    return NotFound(new { message = "Activity not found", error = "ACTIVITY_NOT_FOUND" });
                }

                // Check authorization: creator or participant
                var hasAccess = activity.CreatedByUserId == userId ||
                               await _context.ActivityUsers.AnyAsync(au => au.ActivityId == id && au.UserId == userId);

                if (!hasAccess)
                {
                    return Forbid();
                }

                // Ensure required navigation properties exist (db consistency). If missing, fail fast.
                var createdByEntity = activity.CreatedBy ?? throw new InvalidOperationException($"Activity {id} missing CreatedBy navigation property");

                return Ok(new ActivityDto
                {
                    Id = activity.Id,
                    Name = activity.Name,
                    Description = activity.Description,
                    StartDate = activity.StartDate,
                    EndDate = activity.EndDate,
                    Location = activity.Location,
                    CreatedAt = activity.CreatedAt,
                    UpdatedAt = activity.UpdatedAt,
                    CreatedBy = new UserDto
                    {
                        Id = createdByEntity.Id,
                        Name = createdByEntity.Name,
                        Email = createdByEntity.Email,
                        Role = createdByEntity.Role,
                        CreatedAt = createdByEntity.CreatedAt,
                        UpdatedAt = createdByEntity.UpdatedAt,
                        IsActive = createdByEntity.IsActive
                    },
                    Tasks = activity.Tasks.Select(t => new TaskItemDto
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
                        AssignedTo = t.AssignedTo is User assigned ? new UserDto
                        {
                            Id = assigned.Id,
                            Name = assigned.Name,
                            Email = assigned.Email,
                            Role = assigned.Role,
                            CreatedAt = assigned.CreatedAt,
                            UpdatedAt = assigned.UpdatedAt,
                            IsActive = assigned.IsActive
                        } : null,
                        CreatedBy = new UserDto
                        {
                            Id = (t.CreatedBy ?? throw new InvalidOperationException($"Task {t.Id} missing CreatedBy navigation")).Id,
                            Name = (t.CreatedBy ?? throw new InvalidOperationException($"Task {t.Id} missing CreatedBy navigation")).Name,
                            Email = (t.CreatedBy ?? throw new InvalidOperationException($"Task {t.Id} missing CreatedBy navigation")).Email,
                            Role = (t.CreatedBy ?? throw new InvalidOperationException($"Task {t.Id} missing CreatedBy navigation")).Role,
                            CreatedAt = (t.CreatedBy ?? throw new InvalidOperationException($"Task {t.Id} missing CreatedBy navigation")).CreatedAt,
                            UpdatedAt = (t.CreatedBy ?? throw new InvalidOperationException($"Task {t.Id} missing CreatedBy navigation")).UpdatedAt,
                            IsActive = (t.CreatedBy ?? throw new InvalidOperationException($"Task {t.Id} missing CreatedBy navigation")).IsActive
                        },
                        Expenses = new List<ExpenseDto>()
                    }).ToList(),
                    Participants = activity.ActivityUsers.Select(p => new ActivityUserDto
                    {
                        ActivityId = p.ActivityId,
                        UserId = p.UserId,
                        IsAdmin = p.IsAdmin,
                        JoinedAt = p.JoinedAt,
                        User = new UserDto
                        {
                            Id = (p.User ?? throw new InvalidOperationException($"ActivityUser {p.ActivityId}-{p.UserId} missing User navigation")).Id,
                            Name = (p.User ?? throw new InvalidOperationException($"ActivityUser {p.ActivityId}-{p.UserId} missing User navigation")).Name,
                            Email = (p.User ?? throw new InvalidOperationException($"ActivityUser {p.ActivityId}-{p.UserId} missing User navigation")).Email,
                            Role = (p.User ?? throw new InvalidOperationException($"ActivityUser {p.ActivityId}-{p.UserId} missing User navigation")).Role,
                            CreatedAt = (p.User ?? throw new InvalidOperationException($"ActivityUser {p.ActivityId}-{p.UserId} missing User navigation")).CreatedAt,
                            UpdatedAt = (p.User ?? throw new InvalidOperationException($"ActivityUser {p.ActivityId}-{p.UserId} missing User navigation")).UpdatedAt,
                            IsActive = (p.User ?? throw new InvalidOperationException($"ActivityUser {p.ActivityId}-{p.UserId} missing User navigation")).IsActive
                        }
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activity {ActivityId}", id);
                return StatusCode(500, new { message = "Internal server error", error = "DATABASE_ERROR" });
            }
        }

        /// <summary>
        /// Create new activity
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ActivityDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<ActivityDto>> CreateActivity([FromBody] CreateActivityDto? createDto)
        {
            if (createDto == null)
            {
                return BadRequest(new { message = "Request body is required", error = "EMPTY_BODY" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid input data", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
            }

            if (createDto.EndDate < createDto.StartDate)
            {
                return BadRequest(new
                {
                    message = "EndDate should be greater than or equal to StartDate",
                    error = "INVALID_DATE_RANGE"
                });
            }

            if (!ValidationHelper.TryGetCurrentUserId(User, out var userId))
            {
                return ValidationHelper.InvalidAuthenticationResponse();
            }

            try
            {
                var activity = new Activity
                {
                    Name = createDto.Name.Trim(),
                    Description = createDto.Description?.Trim(),
                    StartDate = DateTime.SpecifyKind(createDto.StartDate, DateTimeKind.Utc),
                    EndDate = DateTime.SpecifyKind(createDto.EndDate, DateTimeKind.Utc),
                    Location = createDto.Location?.Trim(),
                    CreatedByUserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Activities.Add(activity);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetActivity), new { id = activity.Id }, new ActivityDto
                {
                    Id = activity.Id,
                    Name = activity.Name,
                    Description = activity.Description,
                    StartDate = activity.StartDate,
                    EndDate = activity.EndDate,
                    Location = activity.Location,
                    CreatedAt = activity.CreatedAt
                });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error creating activity for user {UserId}", userId);
                return StatusCode(500, new { message = "Failed to create activity", error = "DATABASE_ERROR" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating activity for user {UserId}", userId);
                return StatusCode(500, new { message = "Internal server error", error = "UNKNOWN_ERROR" });
            }
        }

        /// <summary>
        /// Update activity
        /// </summary>
        /// <param name="id">Activity ID (must be positive)</param>
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> UpdateActivity(int id, [FromBody] UpdateActivityDto? updateDto)
        {
            var validation = ValidationHelper.ValidatePositiveId(id, "activityId");
            if (validation != null) return validation;

            if (updateDto == null)
            {
                return BadRequest(new { message = "Request body is required", error = "EMPTY_BODY" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid input data", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
            }

            if (!ValidationHelper.TryGetCurrentUserId(User, out var userId))
            {
                return ValidationHelper.InvalidAuthenticationResponse();
            }

            try
            {
                var activity = await _context.Activities.FindAsync(id);
                if (activity == null)
                {
                    return NotFound(new { message = "Activity not found", error = "ACTIVITY_NOT_FOUND" });
                }

                if (activity.CreatedByUserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                if (!string.IsNullOrWhiteSpace(updateDto.Name))
                    activity.Name = updateDto.Name.Trim();
                if (!string.IsNullOrWhiteSpace(updateDto.Description))
                    activity.Description = updateDto.Description.Trim();
                if (updateDto.StartDate.HasValue)
                    activity.StartDate = DateTime.SpecifyKind(updateDto.StartDate.Value, DateTimeKind.Utc);
                if (updateDto.EndDate.HasValue)
                    activity.EndDate = DateTime.SpecifyKind(updateDto.EndDate.Value, DateTimeKind.Utc);
                if (!string.IsNullOrWhiteSpace(updateDto.Location))
                    activity.Location = updateDto.Location.Trim();

                activity.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error updating activity {ActivityId}", id);
                return StatusCode(500, new { message = "Failed to update activity", error = "DATABASE_ERROR" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating activity {ActivityId}", id);
                return StatusCode(500, new { message = "Internal server error", error = "UNKNOWN_ERROR" });
            }
        }

        /// <summary>
        /// Delete activity
        /// </summary>
        /// <param name="id">Activity ID (must be positive)</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> DeleteActivity(int id)
        {
            var validation = ValidationHelper.ValidatePositiveId(id, "activityId");
            if (validation != null) return validation;

            if (!ValidationHelper.TryGetCurrentUserId(User, out var userId))
            {
                return ValidationHelper.InvalidAuthenticationResponse();
            }

            try
            {
                var activity = await _context.Activities.FindAsync(id);
                if (activity == null)
                {
                    return NotFound(new { message = "Activity not found", error = "ACTIVITY_NOT_FOUND" });
                }

                if (activity.CreatedByUserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                _context.Activities.Remove(activity);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error deleting activity {ActivityId}", id);
                return StatusCode(500, new { message = "Failed to delete activity", error = "DATABASE_ERROR" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting activity {ActivityId}", id);
                return StatusCode(500, new { message = "Internal server error", error = "UNKNOWN_ERROR" });
            }
        }

        /// <summary>
        /// Remove a participant from an activity
        /// </summary>
        /// <param name="id">Activity ID (must be positive)</param>
        /// <param name="userId">User ID to remove (must be positive)</param>
        [HttpDelete("{id}/participants/{userId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> RemoveParticipant(int id, int userId)
        {
            var activityValidation = ValidationHelper.ValidatePositiveId(id, "activityId");
            if (activityValidation != null) return activityValidation;

            var userValidation = ValidationHelper.ValidatePositiveId(userId, "userId");
            if (userValidation != null) return userValidation;

            if (!ValidationHelper.TryGetCurrentUserId(User, out var currentUserId))
            {
                return ValidationHelper.InvalidAuthenticationResponse();
            }

            try
            {
                var activity = await _context.Activities.FindAsync(id);
                if (activity == null)
                {
                    return NotFound(new { message = "Activity not found", error = "ACTIVITY_NOT_FOUND" });
                }

                // Check if the participant exists
                var activityUser = await _context.ActivityUsers
                    .FirstOrDefaultAsync(au => au.ActivityId == id && au.UserId == userId);

                if (activityUser == null)
                {
                    return NotFound(new { message = "Participant not found in this activity", error = "PARTICIPANT_NOT_FOUND" });
                }

                // Authorization: Only activity creator can remove others, but users can remove themselves
                var isCreator = activity.CreatedByUserId == currentUserId;
                var isSelf = userId == currentUserId;

                if (!isCreator && !isSelf)
                {
                    return StatusCode(403, new { message = "Only the activity creator can remove other participants", error = "FORBIDDEN" });
                }

                // Cannot remove the creator from the activity
                if (userId == activity.CreatedByUserId)
                {
                    return BadRequest(new { message = "Cannot remove the activity creator", error = "CANNOT_REMOVE_CREATOR" });
                }

                _context.ActivityUsers.Remove(activityUser);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error removing participant {UserId} from activity {ActivityId}", userId, id);
                return StatusCode(500, new { message = "Failed to remove participant", error = "DATABASE_ERROR" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error removing participant {UserId} from activity {ActivityId}", userId, id);
                return StatusCode(500, new { message = "Internal server error", error = "UNKNOWN_ERROR" });
            }
        }
    }

    public class CreateActivityDto
    {
        [Required(ErrorMessage = "Activity name is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 200 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description must not exceed 1000 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }

        [StringLength(100, ErrorMessage = "Location must not exceed 100 characters")]
        public string? Location { get; set; }
    }

    public class UpdateActivityDto
    {
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 200 characters")]
        public string? Name { get; set; }

        [StringLength(1000, ErrorMessage = "Description must not exceed 1000 characters")]
        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(100, ErrorMessage = "Location must not exceed 100 characters")]
        public string? Location { get; set; }
    }
}