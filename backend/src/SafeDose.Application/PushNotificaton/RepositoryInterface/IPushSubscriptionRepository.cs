using SafeDose.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeDose.Application.PushNotificaton.RepositoryInterface
{
    public interface IPushSubscriptionRepository
    {
        Task<List<PushSubscription>> GetByAccountId(string accountId);
        Task<bool> IfDeviceExsit(string endPoint);
        Task<string> Add(PushSubscription pushSubscription);
        Task<string> Delete(PushSubscription pushSubscription);
        Task<bool> IsSubscripe(string userId);
    }
}
