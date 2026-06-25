using SafeDose.Application.PushNotificaton.RepositoryInterface;
using SafeDose.Domain.ApplicationDbContext;
using SafeDose.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeDose.Infrastructure.PushNotificaton.RepositoryImplementation
{
    public class PushSubscriptionRepository : IPushSubscriptionRepository
    {
        private readonly AppDbContext _context;
        public PushSubscriptionRepository(AppDbContext context)
        {
            _context =  context;
        }

        public async Task<string> Add(PushSubscription pushSubscription)
        {
            await _context.PushSubscription.AddAsync(pushSubscription);    
            await _context.SaveChangesAsync();
            return "تمت الاضافه بنجاح";
        }
    }
}
