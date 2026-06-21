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
            var today = DateOnly.FromDateTime(DateTime.Now);
            var now = TimeOnly.FromDateTime(DateTime.Now);

            //List<PatientMedicationTime>? patientMedicationTimes = await _context.PatientMedicationTimes.Where(
            //t => t.Time <= now && t.LastReminderDate != today &&
            //t.PatientMedication.Status == 1).Include(pm=>pm.PatientMedication).ThenInclude(d=>d.Drug).ToListAsync();


            var ReminderDate = await _context.PatientMedicationTimes.Where(
            t => t.Time <= now && t.LastReminderDate != today &&
            t.PatientMedication.Status == 1).Select(d=>new {
                AccountId = d.AccountId ,
                PatientMedicationTimeId = d.PatientMedicationTimeId,
                PatientMedicationId = d.PatientMedicationId,
                DrugName = d.PatientMedication.Drug.DrugName,
                PatientName = d.PatientMedication.Patient.FullName
            }).ToListAsync();

            foreach (var R in ReminderDate)
            {
                //string drugName = pmt.PatientMedication.Drug.DrugName;
                await SendAsync(R.AccountId, R.PatientMedicationTimeId, "حان وقت العلاج", R.DrugName, R.PatientName, R.PatientMedicationId);
            }

           // await SendAsync("fb02d9b6-1aba-431a-b095-e9372c89da03", 5, "testTitle", "testBody");
        }

        public async Task SendAsync(string AccountId, int PatientMedicationTimeId, string title, string body , string patientName , int PatientMedicationId)
        {
            var subscriptions =
          _context.PushSubscription.Where(p=>p.AccountId == AccountId).ToList();

            var payload = JsonSerializer.Serialize(
                new
                {
                    title,
                    body,
                    patientName,
                    PatientMedicationId
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
            patientMedicationTime.LastReminderDate = DateOnly.FromDateTime(DateTime.Now);
            await _context.SaveChangesAsync();
        }


    }
}
