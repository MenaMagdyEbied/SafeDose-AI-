using SafeDose.Application.Auth.ServicesInterfaces;
using SafeDose.Application.PushNotificaton.DTOs;
using SafeDose.Application.PushNotificaton.RepositoryInterface;
using SafeDose.Application.PushNotificaton.ServicesInterface;
using SafeDose.Domain.Entities;
using SafeDose.Infrastructure.PushNotificaton.RepositoryImplementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeDose.Infrastructure.PushNotificaton.ServicesImplementation
{
    public class PushSubscriptionServices : IPushSubscriptionServices
    {
        private readonly IPushSubscriptionRepository _pushSubscriptionRepository;
        private readonly IUserGlobalServices _userGlobalServices;

        public PushSubscriptionServices(IPushSubscriptionRepository pushSubscriptionRepository , IUserGlobalServices userGlobalServices)
        {
            _pushSubscriptionRepository = pushSubscriptionRepository;
            _userGlobalServices = userGlobalServices;
        }
        public async Task<string> Add(PushSubscriptionAddDTO pushSubscriptionAddModel)
        {
            Account account = await _userGlobalServices.GetUser();
            if (await _pushSubscriptionRepository.IfDeviceExsit(pushSubscriptionAddModel.Endpoint))
                throw new Exception("انت مشترك ب الفعل");
            PushSubscription pushSubscription = new PushSubscription
            {
                Endpoint = pushSubscriptionAddModel.Endpoint,
                P256DH = pushSubscriptionAddModel.P256DH,
                Auth = pushSubscriptionAddModel.Auth,
                AccountId = account.Id                
            };
            string result = await _pushSubscriptionRepository.Add(pushSubscription);
            return result;
        }

        public async Task<string> Delete()
        {
            Account account =await _userGlobalServices.GetUser();
            List<PushSubscription> pushSubscriptions = await _pushSubscriptionRepository.GetByAccountId(account.Id);
            if (pushSubscriptions == null)
                throw new Exception("لا يوجد اي اشتراكات");
            foreach (PushSubscription pushSubscription in pushSubscriptions) {
               await _pushSubscriptionRepository.Delete(pushSubscription);
            }
            return "تم الحزف";
        }

        public async Task<bool> IsSubscripe()
        {
           Account account = await _userGlobalServices.GetUser();
           return await _pushSubscriptionRepository.IsSubscripe(account.Id);    
        }

    }
}
