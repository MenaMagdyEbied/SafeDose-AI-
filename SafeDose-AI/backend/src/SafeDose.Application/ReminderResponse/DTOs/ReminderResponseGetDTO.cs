using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeDose.Application.ReminderResponse.DTOs
{
    public class ReminderResponseGetDTO
    {
        public string DrugName { get; set; }    
        public string ResponsedAt { get; set; }   
        public string ResponsedType { get; set; }   
    }
}
