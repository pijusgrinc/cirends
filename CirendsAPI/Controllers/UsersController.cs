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
    public class UsersController : ControllerBase
    {
        private readonly CirendsDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(CirendsDbContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all users (Admin only)
        /// </summary>
        /// <response code="200">Returns the list of users</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="403">Forbidden - Admin role required</response>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(List<AdminUserDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<List<AdminUserDto>>> GetAllUsers()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new AdminUserDto
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Email = u.Email,
                        Role = u.Role,
                        IsActive = u.IsActive,
                        CreatedAt = u.CreatedAt
                    })
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users");
                return StatusCode(500, new { message = "Internal server error", error = "DATABASE_ERROR" });
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        /// <param name="id">User ID (must be positive)</param>
        /// <response code="200">Returns the user</response>
        /// <response code="400">Invalid ID format (negative or zero)</response>
        /// <response code="404">User not found</response>
        /// <response code="401">Unauthorized access</response>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var validation = ValidationHelper.ValidatePositiveId(id, "userId");
            if (validation != null) return new ActionResult<UserDto>(validation);

            try
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound(new { message = "User not found", error = "USER_NOT_FOUND" });
                }

                return Ok(new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId}", id);
                return StatusCode(500, new { message = "Internal server error", error = "DATABASE_ERROR" });
            }
        }

        /// <summary>
        /// Update user information
        /// </summary>
        /// <param name="id">User ID (must be positive)</param>
        /// <param name="updateDto">User update data</param>
        /// <response code="200">User updated successfully, returns updated user</response>
        /// <response code="400">Invalid input data or ID</response>
        /// <response code="404">User not found</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="403">Can only update own profile or not admin</response>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto? updateDto)
        {
            var validation = ValidationHelper.ValidatePositiveId(id, "userId");
            if (validation != null) return validation;

            if (updateDto == null)
            {
                return BadRequest(new { message = "Request body is required", error = "EMPTY_BODY" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid input data", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
            }

            if (!ValidationHelper.TryGetCurrentUserId(User, out var currentUserId))
            {
                return ValidationHelper.InvalidAuthenticationResponse();
            }

            var isAdmin = User.IsInRole("Admin");

            if (currentUserId != id && !isAdmin)
            {
                return Forbid();
            }

            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found", error = "USER_NOT_FOUND" });
                }

                if (!string.IsNullOrWhiteSpace(updateDto.Name))
                {
                    user.Name = updateDto.Name.Trim();
                }

                if (!string.IsNullOrWhiteSpace(updateDto.Email))
                {
                    var emailTaken = await _context.Users
                        .AsNoTracking()
                        .AnyAsync(u => u.Email == updateDto.Email.ToLower() && u.Id != id);

                    if (emailTaken)
                    {
                        return BadRequest(new { message = "Email is already taken", error = "EMAIL_TAKEN" });
                    }

                    user.Email = updateDto.Email.ToLower().Trim();
                }

                await _context.SaveChangesAsync();
                
                return Ok(new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error updating user {UserId}", id);
                return StatusCode(500, new { message = "Failed to update user", error = "DATABASE_ERROR" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating user {UserId}", id);
                return StatusCode(500, new { message = "Internal server error", error = "UNKNOWN_ERROR" });
            }
        }

        /// <summary>
        /// Delete user (Admin only)
        /// </summary>
        /// <param name="id">User ID (must be positive)</param>
        /// <response code="204">User deleted successfully</response>
        /// <response code="400">Invalid ID format</response>
        /// <response code="404">User not found</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="403">Admin role required</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var validation = ValidationHelper.ValidatePositiveId(id, "userId");
            if (validation != null) return validation;

            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found", error = "USER_NOT_FOUND" });
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error deleting user {UserId}", id);
                return StatusCode(500, new { message = "Failed to delete user", error = "DATABASE_ERROR" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting user {UserId}", id);
                return StatusCode(500, new { message = "Internal server error", error = "UNKNOWN_ERROR" });
            }
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        /// <response code="200">Returns the current user profile</response>
        /// <response code="401">Unauthorized or invalid token</response>
        /// <response code="404">User not found</response>
        [HttpGet("profile")]
        [Authorize]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UserDto>> GetProfile()
        {
            if (!ValidationHelper.TryGetCurrentUserId(User, out var userId))
            {
                return ValidationHelper.InvalidAuthenticationResponse();
            }

            try
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound(new { message = "User not found", error = "USER_NOT_FOUND" });
                }

                return Ok(new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current user profile");
                return StatusCode(500, new { message = "Internal server error", error = "DATABASE_ERROR" });
            }
        }

        /// <summary>
        /// Update user role (Admin only)
        /// </summary>
        [HttpPut("{id}/role")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateRoleRequest request)
        {
            var validation = ValidationHelper.ValidatePositiveId(id, "userId");
            if (validation != null) return validation;

            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.Role))
            {
                return BadRequest(new { message = "Invalid role", error = "INVALID_ROLE" });
            }

            var normalizedRole = request.Role.Trim();
            if (normalizedRole != "Admin" && normalizedRole != "User")
            {
                return BadRequest(new { message = "Role must be 'Admin' or 'User'", error = "INVALID_ROLE" });
            }

            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found", error = "USER_NOT_FOUND" });
                }

                user.Role = normalizedRole;
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user role {UserId}", id);
                return StatusCode(500, new { message = "Failed to update role", error = "DATABASE_ERROR" });
            }
        }

        /// <summary>
        /// Toggle user active status (Admin only)
        /// </summary>
        [HttpPut("{id}/toggle-active")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ToggleUserActive(int id)
        {
            var validation = ValidationHelper.ValidatePositiveId(id, "userId");
            if (validation != null) return validation;

            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found", error = "USER_NOT_FOUND" });
                }

                user.IsActive = !user.IsActive;
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling user active status {UserId}", id);
                return StatusCode(500, new { message = "Failed to toggle active status", error = "DATABASE_ERROR" });
            }
        }

        /// <summary>
        /// Get system statistics (Admin only)
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(SystemStatistics), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<SystemStatistics>> GetStatistics()
        {
            try
            {
                var stats = new SystemStatistics
                {
                    TotalUsers = await _context.Users.CountAsync(),
                    ActiveUsers = await _context.Users.CountAsync(u => u.IsActive),
                    TotalActivities = await _context.Activities.CountAsync(),
                    TotalTasks = await _context.Tasks.CountAsync(),
                    CompletedTasks = await _context.Tasks.CountAsync(t => t.Status == TaskItemStatus.Completed),
                    TotalExpenses = await _context.Expenses.CountAsync(),
                    TotalExpenseAmount = await _context.Expenses.SumAsync(e => (decimal?)e.Amount) ?? 0
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving statistics");
                return StatusCode(500, new { message = "Internal server error", error = "DATABASE_ERROR" });
            }
        }

        /// <summary>
        /// Get all activities with basic info (Admin only)
        /// </summary>
        [HttpGet("activities")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(List<ActivityDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<List<ActivityDto>>> GetAllActivities()
        {
            try
            {
                var activities = await _context.Activities
                    .Where(a => a.CreatedBy != null)
                    .Include(a => a.CreatedBy)
                    .Select(a => new ActivityDto
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Description = a.Description,
                        StartDate = a.StartDate,
                        EndDate = a.EndDate,
                        Location = a.Location,
                        CreatedAt = a.CreatedAt,
                        UpdatedAt = a.UpdatedAt,
                        CreatedBy = new UserDto
                        {
                            Id = a.CreatedBy!.Id,
                            Name = a.CreatedBy.Name,
                            Email = a.CreatedBy.Email,
                            CreatedAt = a.CreatedBy.CreatedAt
                        }
                    })
                    .ToListAsync();

                return Ok(activities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all activities");
                return StatusCode(500, new { message = "Internal server error", error = "DATABASE_ERROR" });
            }
        }
    }

    public class UpdateUserDto
    {
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string? Name { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email must not exceed 100 characters")]
        public string? Email { get; set; }
    }

    public class UpdateRoleRequest
    {
        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; } = string.Empty;
    }

    public class SystemStatistics
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalActivities { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int TotalExpenses { get; set; }
        public decimal TotalExpenseAmount { get; set; }
    }

    public class AdminUserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}