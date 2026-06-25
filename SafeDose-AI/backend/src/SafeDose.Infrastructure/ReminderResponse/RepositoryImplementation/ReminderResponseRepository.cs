using SafeDose.Application.ReminderResponse.RepositoryInterface;
using SafeDose.Domain.ApplicationDbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeDose.Infrastructure.ReminderResponse.RepositoryImplementation
{
    public class ReminderResponseRepository : IReminderResponseRepository
    {
        private readonly AppDbContext _context;
        public ReminderResponseRepository(AppDbContext context)
        {
            _context = context; 
        }
        public async Task<string> Add(Domain.Entities.ReminderResponse reminderResponse)
        {
           await _context.ReminderResponses.AddAsync(reminderResponse);
           await _context.SaveChangesAsync();
           return "تم الرد";
        }

    }
}
