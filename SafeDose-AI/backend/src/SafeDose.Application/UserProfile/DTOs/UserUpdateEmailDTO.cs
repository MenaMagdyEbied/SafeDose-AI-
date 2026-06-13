using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeDose.Application.UserProfile.DTOs
{
    public class UserUpdateEmailDTO
    {
        private string _email;
        [MinLength(1), MaxLength(150)]
        public string Email { get => _email; set => _email = value.Trim(); }   
    }
}
