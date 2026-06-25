using SafeDose.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeDose.Application.ReminderResponse.RepositoryInterface
{
    public interface IReminderResponseRepository
    {
        Task<string> Add(SafeDose.Domain.Entities.ReminderResponse reminderResponse);
    }
}
