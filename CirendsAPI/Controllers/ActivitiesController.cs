using CirendsAPI.Data;
using CirendsAPI.DTOs;
using CirendsAPI.Helpers;
using CirendsAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

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
                        CreatedAt = a.CreatedAt
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
                    .Include(a => a.Tasks)
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

                return Ok(new ActivityDto
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
                    StartDate = createDto.StartDate,
                    EndDate = createDto.EndDate,
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
                    activity.StartDate = updateDto.StartDate.Value;
                if (updateDto.EndDate.HasValue)
                    activity.EndDate = updateDto.EndDate.Value;
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