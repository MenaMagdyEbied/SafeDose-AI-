using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeDose.Application.PushNotificaton.DTOs
{
    public class UserNotifyDto
    {
        public string AccountId { get; set; }
        public string MedicineName { get; set; }    
        public int MedicineId { get; set; }
        public int PatientMedicationId { get; set; }

    }
}
