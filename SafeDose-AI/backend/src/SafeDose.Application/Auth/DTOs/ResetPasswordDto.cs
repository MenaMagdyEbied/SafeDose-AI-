using System.ComponentModel.DataAnnotations;

namespace SafeDose.Application.Auth.DTOs
{
    public class ResetPasswordDto
    {
        [Required , MaxLength(50)]
        [EmailAddress]
        public string Email { get; set; }
        public string Code { get; set; }
        [Required, MaxLength(50)]
        public string NewPassword { get; set; }

        [Required, MaxLength(50)]
        [Compare("NewPassword")]
        public  string ConfirmPassword { get; set; } 
    }
}
