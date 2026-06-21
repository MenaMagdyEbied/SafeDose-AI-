using Microsoft.EntityFrameworkCore;
using SafeDose.Application.Auth.ServicesInterfaces;
using SafeDose.Application.ReminderResponse.DTOs;
using SafeDose.Application.ReminderResponse.RepositoryInterface;
using SafeDose.Application.ReminderResponse.ServicesInterface;
using SafeDose.Domain.ApplicationDbContext;
using SafeDose.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeDose.Infrastructure.ReminderResponse.ServicesImplementation
{
    public class ReminderResponseServices : IReminderResponseServices
    {
        private readonly IReminderResponseRepository _reminderResponseRepository;
        private readonly IUserGlobalServices _userGlobalServices;
        private readonly AppDbContext _context;
        public ReminderResponseServices(AppDbContext context, IReminderResponseRepository reminderResponseRepository , IUserGlobalServices userGlobalServices)
        {
            _context = context; 
            _reminderResponseRepository = reminderResponseRepository;
            _userGlobalServices = userGlobalServices;
        }
        public async Task<string> Add(ReminderResponseAddDTO reminderResponseAddDTO)
        {
            Account account =await _userGlobalServices.GetUser();
            bool flag =  await _context.PatientMedications.AnyAsync(pm=>pm.PatientMedicationId == reminderResponseAddDTO.PatientMedicationId && pm.AccountId == account.Id);
            if (flag == false)
                throw new Exception($"لهذا المستخدم {reminderResponseAddDTO.PatientMedicationId} لم يتم العثور علي الادويه للمريض بالرمز");

            if (reminderResponseAddDTO.ResponseType == 1 || reminderResponseAddDTO.ResponseType == 2 || reminderResponseAddDTO.ResponseType == 3)
            {
                SafeDose.Domain.Entities.ReminderResponse reminderResponse = new Domain.Entities.ReminderResponse
                {
                    PatientMedicationId = reminderResponseAddDTO.PatientMedicationId,
                    AccountId = account.Id,
                    ResponseType = reminderResponseAddDTO.ResponseType,
                    RespondedAt = DateTime.Now
                };

                string result = await _reminderResponseRepository.Add(reminderResponse);
                return result;
            }
            throw new Exception("يجب أن تكون القيمة المدخلة 1 أو 2 أو 3. 1 مقبول، 2 مرفوض، 3 يتم تجاهله");
        }

        public async Task<List<ReminderResponseGetDTO>> GetReminderResponse(int PatientId)
        {
            Account account = await _userGlobalServices.GetUser();
            bool flag = await _context.Patients.AnyAsync(p => p.PatientId == PatientId && p.AccountId == account.Id);
            if (flag == false)
                throw new Exception($"لهذا المستخدم {PatientId} لا يوجد مريض بالرمز");

            var obj = await _context.ReminderResponses.Where(r => r.PatientMedication.Patient.PatientId == PatientId).Select(r => new { DrugName = r.PatientMedication.Drug.DrugName, ResponsedAt = r.RespondedAt, ResponseType = r.ResponseType }).Take(50).ToListAsync();

            List<ReminderResponseGetDTO> reminderResponseGetDTO = new List<ReminderResponseGetDTO>();   
            foreach ( var item in obj )
            {
                string responseType = string.Empty;
                if (item.ResponseType == 1)
                    responseType = "تم اخذ الجرعه";
                else if (item.ResponseType == 2)
                    responseType = "تم رفض الجرعه";
                else if (item.ResponseType == 3)
                    responseType = "تم تجاهل الجرعه";
                else
                    throw new Exception("حدث خطأ ما");

                reminderResponseGetDTO.Add(new ReminderResponseGetDTO
                {
                    DrugName = item.DrugName,
                    ResponsedAt =item.ResponsedAt.ToString(),
                    ResponsedType = responseType
                });
            }

            return reminderResponseGetDTO; 
        }
    }
}
