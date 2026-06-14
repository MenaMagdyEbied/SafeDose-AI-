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

        private string _fullName;
        private string _email;
        private string _userName;
        private string _phoneNumber;


        [MinLength(3),MaxLength(150)]
        public string FullName { get => _fullName; set => _fullName = value.Trim(); }   
        [MinLength(3), MaxLength(80)]
        public string UserName { get => _userName; set => _userName = value.Trim(); }
        [MinLength(1), MaxLength(20)]
        public string PhoneNumber { get => _phoneNumber; set => _phoneNumber = value.Trim(); }
        [EmailAddress , MaxLength(150)]
        public string Email { get => _email; set => _email = value.Trim(); }

        [MaxLength(50)]
        public string Password { get; set; }
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}
