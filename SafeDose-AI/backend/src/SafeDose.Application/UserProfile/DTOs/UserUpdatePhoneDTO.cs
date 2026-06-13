using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeDose.Application.UserProfile.DTOs
{
    public  class UserUpdatePhoneDTO
    {
       
        private string _phone;
        [MinLength(1), MaxLength(20)]
        public string Phone { get => _phone; set => _phone = value.Trim(); }
    }
}
