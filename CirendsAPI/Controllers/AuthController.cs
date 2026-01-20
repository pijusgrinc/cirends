using CirendsAPI.Data;
using CirendsAPI.DTOs;
using CirendsAPI.Models;
using CirendsAPI.Services;
using CirendsAPI.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace CirendsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private const string AccessTokenCookieName = "access_token";
        private const string RefreshTokenCookieName = "refresh_token";

        private readonly CirendsDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(CirendsDbContext context, ITokenService tokenService, ILogger<AuthController> logger)
        {
            _context = context;
            _tokenService = tokenService;
            _logger = logger;
        }

        private CookieOptions BuildCookieOptions(TimeSpan lifetime, bool httpOnly = true)
        {
            // On HTTP (local dev) SameSite=None cookies would be dropped because they require Secure.
            // Relax to Lax when not using HTTPS so the cookie is accepted locally.
            var isHttps = HttpContext.Request.IsHttps;
            var sameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax;

            return new CookieOptions
            {
                HttpOnly = httpOnly,
                Secure = isHttps,
                SameSite = sameSite,
                Expires = DateTimeOffset.UtcNow.Add(lifetime),
                Path = "/"
            };
        }

        private void SetAuthCookies(string accessToken, string refreshToken)
        {
            Response.Cookies.Append(
                AccessTokenCookieName,
                accessToken,
                BuildCookieOptions(TimeSpan.FromMinutes(60)));

            Response.Cookies.Append(
                RefreshTokenCookieName,
                refreshToken,
                BuildCookieOptions(TimeSpan.FromDays(7)));
        }

        private void ClearAuthCookies()
        {
            Response.Cookies.Delete(AccessTokenCookieName, new CookieOptions { Path = "/" });
            Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions { Path = "/" });
        }

        /// <summary>
        /// Register new user
        /// </summary>
        /// <response code="201">User registered successfully</response>
        /// <response code="400">Invalid input or email already exists</response>
        /// <response code="422">Passwords do not match</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Register([FromBody] RegisterDto? registerDto)
        {
            if (registerDto == null)
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

            try
            {
                var emailExists = await _context.Users
                    .AsNoTracking()
                    .AnyAsync(u => u.Email.ToLower() == registerDto.Email.ToLower());

                if (emailExists)
                {
                    return BadRequest(new
                    {
                        message = "Email is already registered",
                        error = "EMAIL_EXISTS"
                    });
                }

                var hashedPassword = PasswordHelper.HashPassword(registerDto.Password);

                var user = new User
                {
                    Name = registerDto.Name.Trim(),
                    Email = registerDto.Email.ToLower().Trim(),
                    PasswordHash = hashedPassword,
                    Role = "User",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var token = _tokenService.GenerateToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken();

                var refreshTokenEntity = new RefreshToken
                {
                    Token = refreshToken,
                    UserId = user.Id,
                    ExpiryDate = DateTime.UtcNow.AddDays(7)
                };

                _context.RefreshTokens.Add(refreshTokenEntity);
                await _context.SaveChangesAsync();

                SetAuthCookies(token, refreshToken);

                var response = new AuthResponseDto
                {
                    Token = token,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        Role = user.Role,
                        CreatedAt = user.CreatedAt
                    }
                };

                _logger.LogInformation("User registered successfully: {Email}", user.Email);
                return CreatedAtAction(nameof(Register), response);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error during registration");
                return StatusCode(500, new { message = "Failed to register user", error = "DATABASE_ERROR" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration");
                return StatusCode(500, new { message = "Internal server error", error = "UNKNOWN_ERROR" });
            }
        }

        /// <summary>
        /// Login user
        /// </summary>
        /// <response code="200">Login successful, token returned</response>
        /// <response code="400">Invalid email or password</response>
        /// <response code="401">User account is inactive</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] LoginDto? loginDto)
        {
            if (loginDto == null)
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

            try
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == loginDto.Email.ToLower());

                if (user == null || !PasswordHelper.VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Failed login attempt for email: {Email}", loginDto.Email);
                    return BadRequest(new
                    {
                        message = "Invalid email or password",
                        error = "INVALID_CREDENTIALS"
                    });
                }

                if (!user.IsActive)
                {
                    return Unauthorized(new
                    {
                        message = "User account is inactive",
                        error = "ACCOUNT_INACTIVE"
                    });
                }

                var token = _tokenService.GenerateToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken();

                var refreshTokenEntity = new RefreshToken
                {
                    Token = refreshToken,
                    UserId = user.Id,
                    ExpiryDate = DateTime.UtcNow.AddDays(7)
                };

                _context.RefreshTokens.Add(refreshTokenEntity);
                await _context.SaveChangesAsync();

                SetAuthCookies(token, refreshToken);

                var response = new AuthResponseDto
                {
                    Token = token,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        Role = user.Role,
                        CreatedAt = user.CreatedAt
                    }
                };

                _logger.LogInformation("User logged in successfully: {Email}", user.Email);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login");
                return StatusCode(500, new { message = "Internal server error", error = "UNKNOWN_ERROR" });
            }
        }

        /// <summary>
        /// Refresh access token
        /// </summary>
        /// <response code="200">Token refreshed successfully</response>
        /// <response code="400">Invalid or expired refresh token</response>
        /// <response code="401">Refresh token is revoked</response>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto? request)
        {
            var refreshToken = Request.Cookies[RefreshTokenCookieName] ?? request?.RefreshToken;
            var accessToken = request?.Token ?? Request.Cookies[AccessTokenCookieName];

            if (string.IsNullOrWhiteSpace(refreshToken) || string.IsNullOrWhiteSpace(accessToken))
            {
                return BadRequest(new { message = "Refresh token or access token is missing", error = "MISSING_TOKEN" });
            }

            if (request != null && !ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Invalid input data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            try
            {
                var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
                if (principal == null)
                {
                    return BadRequest(new { message = "Invalid access token", error = "INVALID_TOKEN" });
                }

                var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                {
                    return BadRequest(new { message = "Invalid token claims", error = "INVALID_CLAIMS" });
                }

                var refreshTokenEntity = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserId == userId);

                if (refreshTokenEntity == null)
                {
                    return BadRequest(new { message = "Invalid refresh token", error = "INVALID_REFRESH_TOKEN" });
                }

                if (refreshTokenEntity.IsRevoked)
                {
                    return Unauthorized(new { message = "Refresh token is revoked", error = "TOKEN_REVOKED" });
                }

                if (refreshTokenEntity.ExpiryDate < DateTime.UtcNow)
                {
                    return BadRequest(new { message = "Refresh token has expired", error = "TOKEN_EXPIRED" });
                }

                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null || !user.IsActive)
                {
                    return Unauthorized(new { message = "User not found or inactive", error = "USER_INVALID" });
                }

                var newAccessToken = _tokenService.GenerateToken(user);
                var newRefreshToken = _tokenService.GenerateRefreshToken();

                // Revoke old refresh token
                refreshTokenEntity.IsRevoked = true;
                _context.RefreshTokens.Update(refreshTokenEntity);

                // Add new refresh token
                var newRefreshTokenEntity = new RefreshToken
                {
                    Token = newRefreshToken,
                    UserId = user.Id,
                    ExpiryDate = DateTime.UtcNow.AddDays(7)
                };

                _context.RefreshTokens.Add(newRefreshTokenEntity);
                await _context.SaveChangesAsync();

                SetAuthCookies(newAccessToken, newRefreshToken);

                var response = new AuthResponseDto
                {
                    Token = newAccessToken,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        Role = user.Role,
                        CreatedAt = user.CreatedAt
                    }
                };

                _logger.LogInformation("Token refreshed successfully for user: {UserId}", userId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during token refresh");
                return StatusCode(500, new { message = "Internal server error", error = "UNKNOWN_ERROR" });
            }
        }

        /// <summary>
        /// Logout user and revoke refresh token
        /// </summary>
        /// <response code="200">Logout successful</response>
        /// <response code="400">Invalid refresh token</response>
        [HttpPost("logout")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto? request)
        {
            try
            {
                var refreshToken = Request.Cookies[RefreshTokenCookieName] ?? request?.RefreshToken;
                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    return BadRequest(new { message = "Refresh token is missing", error = "MISSING_REFRESH_TOKEN" });
                }

                var refreshTokenEntity = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

                if (refreshTokenEntity == null)
                {
                    return BadRequest(new { message = "Invalid refresh token", error = "INVALID_REFRESH_TOKEN" });
                }

                refreshTokenEntity.IsRevoked = true;
                _context.RefreshTokens.Update(refreshTokenEntity);
                await _context.SaveChangesAsync();

                ClearAuthCookies();

                _logger.LogInformation("User logged out successfully. Refresh token revoked for user: {UserId}", refreshTokenEntity.UserId);
                return Ok(new { message = "Logout successful" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during logout");
                return StatusCode(500, new { message = "Internal server error", error = "UNKNOWN_ERROR" });
            }
        }
    }
}