using SafeDose.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeDose.Application.ReminderResponse.DTOs
{
    public class ReminderResponseAddDTO
    {
        public int PatientMedicationId { get; set; }

        public byte ResponseType { get; set; }
    }
}
