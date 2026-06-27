using SafeDose.Application.ReminderResponse.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeDose.Application.ReminderResponse.ServicesInterface
{
    public interface IReminderResponseServices
    {
        Task<string> Add(ReminderResponseAddDTO reminderResponseAddDTO);
        Task<List<ReminderResponseGetDTO>> GetReminderResponse(int PatientId);
    }
}
