using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeDose.Application.UserProfile.DTOs
{
    public class UserUpdateNameDTO
    {
        private string _name;
        [MinLength(1), MaxLength(150)]
        public string Name { get => _name; set => _name = value.Trim(); }        
    }
}
