using System.ComponentModel.DataAnnotations;

namespace CirendsAPI.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Vardas yra b�tinas")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Vardas turi b�ti nuo 2 iki 100 simboli�")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El. pa�to adresas yra b�tinas")]
        [EmailAddress(ErrorMessage = "Neteisingas el. pa�to formatas")]
        [StringLength(255, ErrorMessage = "El. pa�to adresas negali vir�yti 255 simboli�")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slapta�odis yra b�tinas")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Slapta�odis turi b�ti nuo 6 iki 255 simboli�")]
        public string Password { get; set; } = string.Empty;

        public string? Role { get; internal set; }
    }

    public class LoginDto
    {
        [Required(ErrorMessage = "El. pa�to adresas yra b�tinas")]
        [EmailAddress(ErrorMessage = "Neteisingas el. pa�to formatas")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slapta�odis yra b�tinas")]
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
        public UserDto User { get; set; } = null!;
    }

    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Dabartinis slapta�odis yra b�tinas")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Naujas slapta�odis yra b�tinas")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Slapta�odis turi b�ti nuo 6 iki 255 simboli�")]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class RefreshTokenRequestDto
    {
        [Required(ErrorMessage = "Žetonas yra būtinas")]
        public string Token { get; set; } = string.Empty;

        public string? RefreshToken { get; set; }
    }
}