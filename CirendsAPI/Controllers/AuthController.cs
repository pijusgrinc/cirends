using CirendsAPI.Data;
using CirendsAPI.DTOs;
using CirendsAPI.Models;
using CirendsAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace CirendsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly CirendsDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(CirendsDbContext context, ITokenService tokenService, ILogger<AuthController> logger)
        {
            _context = context;
            _tokenService = tokenService;
            _logger = logger;
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

                var hashedPassword = HashPassword(registerDto.Password);

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

                if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
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
        /// Hash password using PBKDF2 with SHA256
        /// </summary>
        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var saltBytes = new byte[16];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(saltBytes);
                }

                var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100000, HashAlgorithmName.SHA256);
                var hashBytes = pbkdf2.GetBytes(20);

                var hashWithSalt = new byte[36];
                Array.Copy(saltBytes, 0, hashWithSalt, 0, 16);
                Array.Copy(hashBytes, 0, hashWithSalt, 16, 20);

                return Convert.ToBase64String(hashWithSalt);
            }
        }

        /// <summary>
        /// Verify password
        /// </summary>
        private static bool VerifyPassword(string password, string hash)
        {
            var hashBytes = Convert.FromBase64String(hash);
            var saltBytes = new byte[16];
            Array.Copy(hashBytes, 0, saltBytes, 0, 16);

            var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100000, HashAlgorithmName.SHA256);
            var hashBytes2 = pbkdf2.GetBytes(20);

            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != hashBytes2[i])
                    return false;
            }

            return true;
        }
    }
}