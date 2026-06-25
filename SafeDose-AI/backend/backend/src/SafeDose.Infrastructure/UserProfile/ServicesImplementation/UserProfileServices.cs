using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SafeDose.Application.Auth.DTOs;
using SafeDose.Application.Auth.ServicesInterfaces;
using SafeDose.Application.UserProfile.DTOs;
using SafeDose.Application.UserProfile.RepositoryInterface;
using SafeDose.Application.UserProfile.ServicesInterface;
using SafeDose.Domain.ApplicationDbContext;
using SafeDose.Domain.Entities;
using SafeDose.Infrastructure.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeDose.Infrastructure.UserProfile.ServicesImplementation
{
    public class UserProfileServices : IUserProfileServices
    {
        private readonly AppDbContext _context;
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly UserManager<Account> _userManager;
        private readonly IUserGlobalServices _userGlobalServices;    
        public UserProfileServices(AppDbContext context,IUserProfileRepository userProfileRepository , UserManager<Account> userManager , IUserGlobalServices userGlobalServices)
        {
            _context = context;
            _userProfileRepository = userProfileRepository;
            _userManager = userManager;
            _userGlobalServices = userGlobalServices;   
        }
        public async Task<UserGetProfileDTO> GetUserProfile()
        {
            Account account = await _userGlobalServices.GetUser();
            List<string> accountRoles = _userManager.GetRolesAsync(account).Result.ToList(); 
            UserGetProfileDTO userGetProfileDTO = new UserGetProfileDTO
            {
                Name = account.Name,    
                UserName = account.UserName,    
                Email = account.Email,
                Phone = account.PhoneNumber,
                Roles = accountRoles
            };

            return userGetProfileDTO;   
        }



        public async  Task<string> UpdateEmail(UserUpdateEmailDTO userUpdateEmail)
        {
            Account account = await _userGlobalServices.GetUser(); 

            if (await _userManager.FindByEmailAsync(userUpdateEmail.Email) is not null)
                return "الأيميل تم تسجيله  بالفعل";


            var token = await _userManager.GenerateChangeEmailTokenAsync(account, userUpdateEmail.Email);

            var result = await _userManager.ChangeEmailAsync(account, userUpdateEmail.Email, token);

            if (result.Succeeded)
                return "تم الاستبدال بنجاح";

            else
                throw new Exception(result.Errors.ToString());
        }

        public async Task<string> UpdateName(UserUpdateNameDTO userUpdateName)
        {
            Account account = await _userGlobalServices.GetUser();
            account.Name = userUpdateName.Name; 
            await _userProfileRepository.UpdateUser(account);
            return "تم الاستبدال بنجاح";
        }

        public async Task<string> UpdatePhone(UserUpdatePhoneDTO userUpdatePhone)
        {
            Account account = await _userGlobalServices.GetUser();
            List<Account>? accounts = await _userManager.Users.Where(u => u.PhoneNumber == userUpdatePhone.Phone).ToListAsync();

            if (accounts.Count() > 0)
                return "رقم الهاتف مسجل بالفعل";

            var token = await _userManager.GenerateChangePhoneNumberTokenAsync(account, userUpdatePhone.Phone);

            var result = await _userManager.ChangePhoneNumberAsync(account, userUpdatePhone.Phone, token);

            if (result.Succeeded)
                return "تم الاستبدال بنجاح";

            else
                throw new Exception(result.Errors.ToString());
        }


        public async Task<string> SetRunningPatient(int patientId)
        {
            Account account = await _userGlobalServices.GetUser();
            Patient? patient = await _context.Patients.SingleOrDefaultAsync(p => p.PatientId == patientId && p.AccountId == account.Id);
            if (patient == null)
                throw new Exception($"لهذا المستخدم {patientId} لا يوجد مريض  ب الرقم التعريف هزا ");

            List<Patient> patients = await _context.Patients.Where(p=>p.AccountId == account.Id).ToListAsync();
            foreach(var p in  patients)
            {
                p.IsRunning = false;    
            }

            patient.IsRunning = true;


            await _context.SaveChangesAsync();     
            return "تم التفعيل";
        }

        public async Task<int> GetRunningPatient()
        {
            Account account = await _userGlobalServices.GetUser();
            Patient? patient = await _context.Patients.SingleOrDefaultAsync(p => p.IsRunning == true && p.AccountId == account.Id);
            if (patient == null)
                throw new Exception("لا يوجد مريض مفعل يجب التفعيل اولا");
            return patient.PatientId;
        }
    }
}
