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
    [Route("api/[controller]")]
    [Authorize]
    public class ActivitiesController : ControllerBase
    {
        private readonly CirendsDbContext _context;

        public ActivitiesController(CirendsDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ActivityDto>>> GetActivities()
        {
            var userId = GetCurrentUserId();
            
            var activities = await _context.Activities
                .Include(a => a.CreatedBy)
                .Include(a => a.ActivityUsers)
                .ThenInclude(au => au.User)
                .Include(a => a.Tasks)
                .ThenInclude(t => t.AssignedTo)
                .Include(a => a.Tasks)
                .ThenInclude(t => t.CreatedBy)
                .Include(a => a.Expenses)
                .ThenInclude(e => e.PaidBy)
                .Where(a => a.CreatedByUserId == userId || a.ActivityUsers.Any(au => au.UserId == userId))
                .ToListAsync();

            var activityDtos = activities.Select(a => new ActivityDto
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
                    Id = a.CreatedBy.Id,
                    Name = a.CreatedBy.Name,
                    Email = a.CreatedBy.Email,
                    CreatedAt = a.CreatedBy.CreatedAt
                },
                Participants = a.ActivityUsers.Select(au => new UserDto
                {
                    Id = au.User.Id,
                    Name = au.User.Name,
                    Email = au.User.Email,
                    CreatedAt = au.User.CreatedAt
                }).ToList(),
                Tasks = a.Tasks.Select(t => new TaskItemDto
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
                    }
                }).ToList(),
                Expenses = a.Expenses.Select(e => new ExpenseDto
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

            return Ok(activityDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ActivityDto>> GetActivity(int id)
        {
            var userId = GetCurrentUserId();
            
            var activity = await _context.Activities
                .Include(a => a.CreatedBy)
                .Include(a => a.ActivityUsers)
                .ThenInclude(au => au.User)
                .Include(a => a.Tasks)
                .ThenInclude(t => t.AssignedTo)
                .Include(a => a.Tasks)
                .ThenInclude(t => t.CreatedBy)
                .Include(a => a.Expenses)
                .ThenInclude(e => e.PaidBy)
                .Include(a => a.Expenses)
                .ThenInclude(e => e.ExpenseShares)
                .ThenInclude(es => es.User)
                .Where(a => a.Id == id && (a.CreatedByUserId == userId || a.ActivityUsers.Any(au => au.UserId == userId)))
                .FirstOrDefaultAsync();

            if (activity == null)
            {
                return NotFound();
            }

            var activityDto = new ActivityDto
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
                    Id = activity.CreatedBy.Id,
                    Name = activity.CreatedBy.Name,
                    Email = activity.CreatedBy.Email,
                    CreatedAt = activity.CreatedBy.CreatedAt
                },
                Participants = activity.ActivityUsers.Select(au => new UserDto
                {
                    Id = au.User.Id,
                    Name = au.User.Name,
                    Email = au.User.Email,
                    CreatedAt = au.User.CreatedAt
                }).ToList(),
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
                    }
                }).ToList(),
                Expenses = activity.Expenses.Select(e => new ExpenseDto
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
                }).ToList()
            };

            return Ok(activityDto);
        }

        [HttpPost]
        public async Task<ActionResult<ActivityDto>> CreateActivity(CreateActivityDto createActivityDto)
        {
            var userId = GetCurrentUserId();
            
            var activity = new Activity
            {
                Name = createActivityDto.Name,
                Description = createActivityDto.Description,
                StartDate = createActivityDto.StartDate,
                EndDate = createActivityDto.EndDate,
                Location = createActivityDto.Location,
                CreatedByUserId = userId
            };

            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            // Add creator as participant
            var activityUser = new ActivityUser
            {
                ActivityId = activity.Id,
                UserId = userId,
                IsAdmin = true
            };

            _context.ActivityUsers.Add(activityUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetActivity), new { id = activity.Id }, await GetActivityDto(activity.Id));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateActivity(int id, UpdateActivityDto updateActivityDto)
        {
            var userId = GetCurrentUserId();
            
            var activity = await _context.Activities
                .Include(a => a.ActivityUsers)
                .FirstOrDefaultAsync(a => a.Id == id && (a.CreatedByUserId == userId || a.ActivityUsers.Any(au => au.UserId == userId && au.IsAdmin)));

            if (activity == null)
            {
                return NotFound();
            }

            if (updateActivityDto.Name != null) activity.Name = updateActivityDto.Name;
            if (updateActivityDto.Description != null) activity.Description = updateActivityDto.Description;
            if (updateActivityDto.StartDate.HasValue) activity.StartDate = updateActivityDto.StartDate.Value;
            if (updateActivityDto.EndDate.HasValue) activity.EndDate = updateActivityDto.EndDate.Value;
            if (updateActivityDto.Location != null) activity.Location = updateActivityDto.Location;
            
            activity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteActivity(int id)
        {
            var userId = GetCurrentUserId();
            
            var activity = await _context.Activities
                .FirstOrDefaultAsync(a => a.Id == id && a.CreatedByUserId == userId);

            if (activity == null)
            {
                return NotFound();
            }

            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id}/participants/{participantId}")]
        public async Task<IActionResult> AddParticipant(int id, int participantId)
        {
            var userId = GetCurrentUserId();
            
            var activity = await _context.Activities
                .Include(a => a.ActivityUsers)
                .FirstOrDefaultAsync(a => a.Id == id && (a.CreatedByUserId == userId || a.ActivityUsers.Any(au => au.UserId == userId && au.IsAdmin)));

            if (activity == null)
            {
                return NotFound("Activity not found or no admin access");
            }

            var participant = await _context.Users.FindAsync(participantId);
            if (participant == null)
            {
                return NotFound("User not found");
            }

            if (activity.ActivityUsers.Any(au => au.UserId == participantId))
            {
                return BadRequest("User already participant");
            }

            var activityUser = new ActivityUser
            {
                ActivityId = id,
                UserId = participantId,
                IsAdmin = false
            };

            _context.ActivityUsers.Add(activityUser);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}/participants/{participantId}")]
        public async Task<IActionResult> RemoveParticipant(int id, int participantId)
        {
            var userId = GetCurrentUserId();
            
            var activity = await _context.Activities
                .Include(a => a.ActivityUsers)
                .FirstOrDefaultAsync(a => a.Id == id && (a.CreatedByUserId == userId || a.ActivityUsers.Any(au => au.UserId == userId && au.IsAdmin)));

            if (activity == null)
            {
                return NotFound("Activity not found or no admin access");
            }

            var activityUser = activity.ActivityUsers.FirstOrDefault(au => au.UserId == participantId);
            if (activityUser == null)
            {
                return NotFound("Participant not found");
            }

            // Prevent removing the creator
            if (activity.CreatedByUserId == participantId)
            {
                return BadRequest("Cannot remove activity creator");
            }

            _context.ActivityUsers.Remove(activityUser);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<ActivityDto?> GetActivityDto(int id)
        {
            var activity = await _context.Activities
                .Include(a => a.CreatedBy)
                .Include(a => a.ActivityUsers)
                .ThenInclude(au => au.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (activity == null) return null;

            return new ActivityDto
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
                    Id = activity.CreatedBy.Id,
                    Name = activity.CreatedBy.Name,
                    Email = activity.CreatedBy.Email,
                    CreatedAt = activity.CreatedBy.CreatedAt
                },
                Participants = activity.ActivityUsers.Select(au => new UserDto
                {
                    Id = au.User.Id,
                    Name = au.User.Name,
                    Email = au.User.Email,
                    CreatedAt = au.User.CreatedAt
                }).ToList()
            };
        }
    }
}