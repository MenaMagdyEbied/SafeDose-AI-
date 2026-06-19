using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SafeDose.Application.PushNotificaton.DTOs;
using SafeDose.Application.PushNotificaton.ServicesInterface;
using SafeDose.Infrastructure.PushNotificaton.ServicesImplementation;

namespace SafeDose.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PushSubscriptionController : ControllerBase
    {
        private readonly IPushSubscriptionServices _pushSubscriptionServices;
        private readonly INotificationServices _notificationServices;   
        public PushSubscriptionController(IPushSubscriptionServices pushSubscriptionServices , INotificationServices notificationServices)
        {
            _pushSubscriptionServices = pushSubscriptionServices;   
            _notificationServices = notificationServices;
        }

        [HttpPost("PushSubscription")]
        public async Task<IActionResult> PushSubscription([FromBody] PushSubscriptionAddDTO dto)
        {
            try
            {
                string result =await _pushSubscriptionServices.Add(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);  
            }
        }

        [HttpPost]
        public async Task<IActionResult> test()
        {
            RecurringJob.AddOrUpdate<NotificationServices>(x=>x.UserWillBeNotify(), Cron.Minutely);
          //  await _notificationServices.UserWillBeNotify();
            return Ok();    
        }
    }
}
