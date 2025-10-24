using Microsoft.AspNetCore.Mvc;

namespace CirendsAPI.Helpers
{
    public static class ValidationHelper
    {
        /// <summary>
        /// Validates that ID is a positive integer
        /// </summary>
        public static ActionResult? ValidatePositiveId(int id, string paramName = "id")
        {
            if (id <= 0)
            {
                return new BadRequestObjectResult(new
                {
                    message = $"{paramName} must be a positive integer",
                    error = "INVALID_ID_FORMAT"
                });
            }
            return null;
        }

        /// <summary>
        /// Validates multiple IDs in hierarchy (Activity -> Task -> Expense)
        /// </summary>
        public static ActionResult? ValidateHierarchyIds(params (int id, string name)[] ids)
        {
            foreach (var (id, name) in ids)
            {
                if (id <= 0)
                {
                    return new BadRequestObjectResult(new
                    {
                        message = $"{name} must be a positive integer",
                        error = "INVALID_ID_FORMAT"
                    });
                }
            }
            return null;
        }

        /// <summary>
        /// Safely parse user ID from claims
        /// </summary>
        public static bool TryGetCurrentUserId(System.Security.Claims.ClaimsPrincipal user, out int userId)
        {
            userId = 0;
            var claim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(claim) || !int.TryParse(claim, out var parsedId) || parsedId <= 0)
            {
                return false;
            }

            userId = parsedId;
            return true;
        }

        /// <summary>
        /// Returns proper error response for invalid auth
        /// </summary>
        public static ActionResult InvalidAuthenticationResponse()
        {
            return new UnauthorizedObjectResult(new
            {
                message = "Invalid authentication token",
                error = "INVALID_TOKEN"
            });
        }
    }
}