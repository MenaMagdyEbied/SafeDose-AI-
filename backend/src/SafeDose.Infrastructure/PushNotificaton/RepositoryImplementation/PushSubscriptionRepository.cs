using Microsoft.EntityFrameworkCore;
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

        public async Task<List<PushSubscription>> GetByAccountId(string accountId)
        {
            List<PushSubscription> pushSubscriptions = await _context.PushSubscription.Where(s=>s.AccountId == accountId).ToListAsync();
            return pushSubscriptions;   
        }

        public async Task<string> Add(PushSubscription pushSubscription)
        {
            await _context.PushSubscription.AddAsync(pushSubscription);    
            await _context.SaveChangesAsync();
            return "تمت الاضافه بنجاح";
        }

        public async Task<string> Delete(PushSubscription pushSubscription)
        {
             _context.PushSubscription.Remove(pushSubscription);
            await _context.SaveChangesAsync();
            return "تمت الحزف";
        }

        public async Task<bool> IsSubscripe(string userId)
        {
            bool isSubscripe = await _context.PushSubscription.AnyAsync(s=>s.AccountId == userId);
            return isSubscripe;
        }

        public async Task<bool> IfDeviceExsit(string endPoint)
        {
            bool isSubscripe = await _context.PushSubscription.AnyAsync(s => s.Endpoint == endPoint);
            return isSubscripe;
        }
    }
}
