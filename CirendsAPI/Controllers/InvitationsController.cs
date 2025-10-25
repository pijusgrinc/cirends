using CirendsAPI.Data;
using CirendsAPI.DTOs;
using CirendsAPI.Helpers;
using CirendsAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CirendsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InvitationsController : ControllerBase
    {
        private readonly CirendsDbContext _context;
        private readonly ILogger<InvitationsController> _logger;

        public InvitationsController(CirendsDbContext context, ILogger<InvitationsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Invite user to activity
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(InvitationDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<InvitationDto>> InviteUser([FromBody] CreateInvitationDto? dto)
        {
            if (dto == null)
            {
                return BadRequest(new { message = "Request body is required", error = "EMPTY_BODY" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Invalid input data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var validation = ValidationHelper.ValidatePositiveId(dto.ActivityId, "activityId");
            if (validation != null) return new ActionResult<InvitationDto>(validation);

            if (!ValidationHelper.TryGetCurrentUserId(User, out var userId))
            {
                return ValidationHelper.InvalidAuthenticationResponse();
            }

            try
            {
                // Patikrinti, ar veikla egzistuoja
                var activity = await _context.Activities
                    .Include(a => a.ActivityUsers)
                    .FirstOrDefaultAsync(a => a.Id == dto.ActivityId);

                if (activity == null)
                {
                    return NotFound(new { message = "Activity not found", error = "ACTIVITY_NOT_FOUND" });
                }

                // Patikrinti, ar vartotojas yra veiklos narys arba administratorius
                var isActivityMember = activity.ActivityUsers.Any(au => au.UserId == userId);
                var isAdmin = User.IsInRole("Admin");

                if (!isActivityMember && !isAdmin)
                {
                    return Forbid();
                }

                // Rasti kvieciamą vartotoją
                var invitedUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

                if (invitedUser == null)
                {
                    return BadRequest(new { message = "User with this email not found", error = "USER_NOT_FOUND" });
                }

                // Patikrinti, ar vartotojas jau yra veiklos narys
                if (activity.ActivityUsers.Any(au => au.UserId == invitedUser.Id))
                {
                    return BadRequest(new { message = "User is already a member of this activity", error = "ALREADY_MEMBER" });
                }

                // Patikrinti, ar pakvietimas jau egzistuoja
                var existingInvitation = await _context.Invitations
                    .FirstOrDefaultAsync(i => i.ActivityId == dto.ActivityId &&
                                            i.InvitedUserId == invitedUser.Id &&
                                            i.Status == InvitationStatus.Pending);

                if (existingInvitation != null)
                {
                    return BadRequest(new { message = "Invitation already exists", error = "INVITATION_EXISTS" });
                }

                var invitation = new Invitation
                {
                    ActivityId = dto.ActivityId,
                    InvitedUserId = invitedUser.Id,
                    InvitedByUserId = userId,
                    Status = InvitationStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Invitations.Add(invitation);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {UserId} invited {InvitedUserId} to activity {ActivityId}",
                    userId, invitedUser.Id, dto.ActivityId);

                return CreatedAtAction(nameof(GetInvitation), new { id = invitation.Id },
                    await GetInvitationDto(invitation.Id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inviting user to activity");
                return StatusCode(500, new { message = "Internal server error", error = "DATABASE_ERROR" });
            }
        }

        /// <summary>
        /// Get user's invitations
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<InvitationDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<List<InvitationDto>>> GetMyInvitations()
        {
            if (!ValidationHelper.TryGetCurrentUserId(User, out var userId))
            {
                return ValidationHelper.InvalidAuthenticationResponse();
            }

            try
            {
                var invitations = await _context.Invitations
                    .Where(i => i.InvitedUserId == userId)
                    .Include(i => i.Activity)
                    .Include(i => i.InvitedBy)
                    .ToListAsync();

                var invitationDtos = invitations.Select(i => new InvitationDto
                {
                    Id = i.Id,
                    ActivityId = i.ActivityId,
                    ActivityName = i.Activity?.Name ?? "Unknown",
                    InvitedBy = i.InvitedBy != null ? new UserDto
                    {
                        Id = i.InvitedBy.Id,
                        Name = i.InvitedBy.Name,
                        Email = i.InvitedBy.Email,
                        CreatedAt = i.InvitedBy.CreatedAt
                    } : null,
                    Status = i.Status,
                    CreatedAt = i.CreatedAt,
                    RespondedAt = i.RespondedAt
                }).ToList();

                return Ok(invitationDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invitations for user {UserId}", userId);
                return StatusCode(500, new { message = "Internal server error", error = "DATABASE_ERROR" });
            }
        }

        /// <summary>
        /// Respond to invitation
        /// </summary>
        [HttpPost("{id}/respond")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RespondToInvitation(int id, [FromBody] RespondToInvitationDto? dto)
        {
            if (dto == null)
            {
                return BadRequest(new { message = "Request body is required", error = "EMPTY_BODY" });
            }

            var validation = ValidationHelper.ValidatePositiveId(id, "invitationId");
            if (validation != null) return validation;

            if (!ValidationHelper.TryGetCurrentUserId(User, out var userId))
            {
                return ValidationHelper.InvalidAuthenticationResponse();
            }

            try
            {
                var invitation = await _context.Invitations
                    .Include(i => i.Activity)
                    .FirstOrDefaultAsync(i => i.Id == id && i.InvitedUserId == userId);

                if (invitation == null)
                {
                    return NotFound(new { message = "Invitation not found", error = "INVITATION_NOT_FOUND" });
                }

                if (invitation.Status != InvitationStatus.Pending)
                {
                    return BadRequest(new { message = "Invitation already responded to", error = "ALREADY_RESPONDED" });
                }

                invitation.Status = dto.Accept ? InvitationStatus.Accepted : InvitationStatus.Rejected;
                invitation.RespondedAt = DateTime.UtcNow;

                if (dto.Accept)
                {
                    // Pridėti vartotoją prie veiklos
                    var activityUser = new ActivityUser
                    {
                        ActivityId = invitation.ActivityId,
                        UserId = userId,
                        IsAdmin = false,
                        JoinedAt = DateTime.UtcNow
                    };

                    _context.ActivityUsers.Add(activityUser);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("User {UserId} responded to invitation {InvitationId}: {Accept}",
                    userId, id, dto.Accept);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error responding to invitation {InvitationId}", id);
                return StatusCode(500, new { message = "Internal server error", error = "DATABASE_ERROR" });
            }
        }

        /// <summary>
        /// Get invitation by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(InvitationDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<InvitationDto>> GetInvitation(int id)
        {
            var validation = ValidationHelper.ValidatePositiveId(id, "invitationId");
            if (validation != null) return new ActionResult<InvitationDto>(validation);

            try
            {
                var dto = await GetInvitationDto(id);
                return Ok(dto);
            }
            catch (ArgumentException)
            {
                return NotFound(new { message = "Invitation not found", error = "INVITATION_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invitation {InvitationId}", id);
                return StatusCode(500, new { message = "Internal server error", error = "DATABASE_ERROR" });
            }
        }

        private async Task<InvitationDto> GetInvitationDto(int invitationId)
        {
            var invitation = await _context.Invitations
                .Include(i => i.Activity)
                .Include(i => i.InvitedUser)
                .Include(i => i.InvitedBy)
                .FirstOrDefaultAsync(i => i.Id == invitationId);

            if (invitation == null)
                throw new ArgumentException("Invitation not found");

            return new InvitationDto
            {
                Id = invitation.Id,
                ActivityId = invitation.ActivityId,
                ActivityName = invitation.Activity?.Name ?? "Unknown",
                InvitedUser = invitation.InvitedUser != null ? new UserDto
                {
                    Id = invitation.InvitedUser.Id,
                    Name = invitation.InvitedUser.Name,
                    Email = invitation.InvitedUser.Email,
                    CreatedAt = invitation.InvitedUser.CreatedAt
                } : null,
                InvitedBy = invitation.InvitedBy != null ? new UserDto
                {
                    Id = invitation.InvitedBy.Id,
                    Name = invitation.InvitedBy.Name,
                    Email = invitation.InvitedBy.Email,
                    CreatedAt = invitation.InvitedBy.CreatedAt
                } : null,
                Status = invitation.Status,
                CreatedAt = invitation.CreatedAt,
                RespondedAt = invitation.RespondedAt
            };
        }
    }
}