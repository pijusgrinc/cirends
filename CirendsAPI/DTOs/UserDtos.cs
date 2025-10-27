using System.ComponentModel.DataAnnotations;

namespace CirendsAPI.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Vardas yra bûtinas")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Vardas turi bûti nuo 2 iki 100 simboliø")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El. paðto adresas yra bûtinas")]
        [EmailAddress(ErrorMessage = "Neteisingas el. paðto formatas")]
        [StringLength(255, ErrorMessage = "El. paðto adresas negali virðyti 255 simboliø")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slaptaþodis yra bûtinas")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Slaptaþodis turi bûti nuo 6 iki 255 simboliø")]
        public string Password { get; set; } = string.Empty;

        public string? Role { get; internal set; }
    }

    public class LoginDto
    {
        [Required(ErrorMessage = "El. paðto adresas yra bûtinas")]
        [EmailAddress(ErrorMessage = "Neteisingas el. paðto formatas")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slaptaþodis yra bûtinas")]
        public string Password { get; set; } = string.Empty;
    }

    public class UserDto
    {
        [Key]
        public int Id { get; set; }

        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [StringLength(50)]
        public string Role { get; set; } = "User";

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public UserDto User { get; set; } = null!;
    }

    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Dabartinis slaptaþodis yra bûtinas")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Naujas slaptaþodis yra bûtinas")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Slaptaþodis turi bûti nuo 6 iki 255 simboliø")]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class RefreshTokenRequestDto
    {
        [Required(ErrorMessage = "Þeton yra bûtinas")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Atnaujinimo þeton yra bûtinas")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}