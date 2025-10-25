using CirendsAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace CirendsAPI.DTOs
{
    public class CreateInvitationDto
    {
        [Required(ErrorMessage = "El. paštas yra būtinas")]
        [EmailAddress(ErrorMessage = "Neteisingas el. pašto formatas")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Veiklos ID yra būtinas")]
        public int ActivityId { get; set; }
    }

    public class InvitationDto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ActivityId { get; set; }

        [Required]
        public string ActivityName { get; set; } = string.Empty;

        [Required]
        public UserDto? InvitedUser { get; set; }

        [Required]
        public UserDto? InvitedBy { get; set; }

        [Required]
        public InvitationStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
    }

    public class RespondToInvitationDto
    {
        [Required]
        public bool Accept { get; set; }
    }
}