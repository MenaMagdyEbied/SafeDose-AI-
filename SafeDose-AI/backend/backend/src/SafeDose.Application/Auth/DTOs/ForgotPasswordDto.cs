using System.ComponentModel.DataAnnotations;

namespace SafeDose.Application.Auth.DTOs
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
