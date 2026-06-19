using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SafeDose.Application.Auth.ServicesInterfaces;
using SafeDose.Application.PushNotificaton.DTOs;
using SafeDose.Application.PushNotificaton.ServicesInterface;
using SafeDose.Domain.ApplicationDbContext;
using SafeDose.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebPush;

namespace SafeDose.Infrastructure.PushNotificaton.ServicesImplementation
{
    public class NotificationServices : INotificationServices
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IUserGlobalServices _userGlobalServices;
        // Add fields for VAPID keys
        private readonly string _publicKey;
        private readonly string _privateKey;

        public NotificationServices(AppDbContext context, IConfiguration configuration, IUserGlobalServices userGlobalServices)
        {
            _context = context;
            _configuration = configuration;

            // Retrieve VAPID keys from configuration
            _publicKey = _configuration["Vapid:publicKey"]!;
            _privateKey = _configuration["Vapid:privateKey"]!;


            _userGlobalServices = userGlobalServices;   
        }


        public async Task UserWillBeNotify()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var now = TimeOnly.FromDateTime(DateTime.UtcNow);
            List<PatientMedicationTime>? patientMedicationTimes = await _context.PatientMedicationTimes.Where(
                t => t.Time <= now && t.LastReminderDate != today && 
                t.PatientMedication.Status == 1).ToListAsync();

            foreach (var pmt in patientMedicationTimes)
            {
                string drugName = pmt.PatientMedication.Drug.DrugName;
                await SendAsync(pmt.AccountId, pmt.PatientMedicationTimeId, "testTitle", drugName);
            }

           // await SendAsync("fb02d9b6-1aba-431a-b095-e9372c89da03", 5, "testTitle", "testBody");
        }

        public async Task SendAsync(string AccountId, int PatientMedicationTimeId, string title, string body)
        {
            var subscriptions =
          _context.PushSubscription.Where(p=>p.AccountId == AccountId).ToList();

            var payload = JsonSerializer.Serialize(
                new
                {
                    title,
                    body,
                    PatientMedicationTimeId
                });

            var vapidDetails = new VapidDetails(
                "mailto:test@test.com",
                _publicKey,
                _privateKey);

            var client = new WebPushClient();

            foreach (var sub in subscriptions)
            {
                var subscription =
                    new WebPush.PushSubscription(
                        sub.Endpoint,
                        sub.P256DH,
                        sub.Auth);

                await client.SendNotificationAsync(
                    subscription,
                    payload,
                    vapidDetails);
            }

            PatientMedicationTime? patientMedicationTime = await _context.PatientMedicationTimes.SingleOrDefaultAsync(pmt => pmt.PatientMedicationTimeId == PatientMedicationTimeId);
            patientMedicationTime.LastReminderDate = DateOnly.FromDateTime(DateTime.UtcNow);
            await _context.SaveChangesAsync();
        }


    }
}
