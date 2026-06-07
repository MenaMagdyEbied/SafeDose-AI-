using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeDose.Application.Auth.DTOs
{
    public class RegisterDTO
    {
        [MaxLength(150)]
        public string FullName {  get; set; }   
        [MaxLength(80)]
        public string UserName { get; set; }
        [MaxLength(20)]
        public string PhoneNumber { get; set; }
        [EmailAddress , MaxLength(150)]
        public string Email { get; set; }

        [MaxLength(50)]
        public string Password { get; set; }
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}
