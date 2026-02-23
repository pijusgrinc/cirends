using System.ComponentModel.DataAnnotations;

namespace CirendsAPI.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Vardas yra būtinas")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Vardas turi būti nuo 2 iki 100 simbolių")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El. pašto adresas yra būtinas")]
        [EmailAddress(ErrorMessage = "Neteisingas el. pašto formatas")]
        [StringLength(255, ErrorMessage = "El. pašto adresas negali viršyti 255 simbolių")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slaptažodis yra būtinas")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Slaptažodis turi bti nuo 6 iki 255 simbolių")]
        public string Password { get; set; } = string.Empty;

        public string? Role { get; internal set; }
    }

    public class LoginDto
    {
        [Required(ErrorMessage = "El. pašto adresas yra būtinas")]
        [EmailAddress(ErrorMessage = "Neteisingas el. pašto formatas")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slaptažodis yra būtinas")]
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
        public UserDto User { get; set; } = null!;
    }

    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Dabartinis slaptažodis yra būtinas")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Naujas slaptažodis yra būtinas")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Slaptažodis turi būti nuo 6 iki 255 simbolių")]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class RefreshTokenRequestDto
    {
        [Required(ErrorMessage = "Žetonas yra būtinas")]
        public string Token { get; set; } = string.Empty;

        public string? RefreshToken { get; set; }
    }
}