using SafeDose.Application.PushNotificaton.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeDose.Application.PushNotificaton.ServicesInterface
{
    public interface INotificationServices
    {
        // get user 
        // send not

        Task UserWillBeNotify();
        Task SendAsync(string AccountId ,int PatientMedicationTimeId, string title, string body);

    }
}
