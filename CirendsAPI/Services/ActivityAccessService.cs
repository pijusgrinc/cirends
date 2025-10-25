using CirendsAPI.Data;
using CirendsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CirendsAPI.Services
{
    public interface IActivityAccessService
    {
        Task ValidateUserAccessAsync(int activityId, int userId);

        Task<Activity> GetActivityWithParticipantsAsync(int activityId, int userId);
    }

    public class ActivityAccessService : IActivityAccessService
    {
        private readonly CirendsDbContext _context;

        public ActivityAccessService(CirendsDbContext context)
        {
            _context = context;
        }

        public async Task ValidateUserAccessAsync(int activityId, int userId)
        {
            var hasAccess = await _context.Activities
                .AnyAsync(a => a.Id == activityId &&
                    (a.CreatedByUserId == userId || a.ActivityUsers.Any(au => au.UserId == userId)));

            if (!hasAccess)
                throw new UnauthorizedAccessException("Activity not found or no access");
        }

        public async Task<Activity> GetActivityWithParticipantsAsync(int activityId, int userId)
        {
            var activity = await _context.Activities
                .Include(a => a.ActivityUsers)
                .FirstOrDefaultAsync(a => a.Id == activityId &&
                    (a.CreatedByUserId == userId || a.ActivityUsers.Any(au => au.UserId == userId)));

            if (activity == null)
                throw new UnauthorizedAccessException("Activity not found or no access");

            return activity;
        }
    }
}