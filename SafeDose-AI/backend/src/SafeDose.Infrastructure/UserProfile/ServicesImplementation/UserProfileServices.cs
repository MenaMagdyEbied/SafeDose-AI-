using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SafeDose.Application.Auth.DTOs;
using SafeDose.Application.Auth.ServicesInterfaces;
using SafeDose.Application.UserProfile.DTOs;
using SafeDose.Application.UserProfile.RepositoryInterface;
using SafeDose.Application.UserProfile.ServicesInterface;
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
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly UserManager<Account> _userManager;
        private readonly IUserGlobalServices _userGlobalServices;    
        public UserProfileServices(IUserProfileRepository userProfileRepository , UserManager<Account> userManager , IUserGlobalServices userGlobalServices)
        {
            _userProfileRepository = userProfileRepository;
            _userManager = userManager;
            _userGlobalServices = userGlobalServices;   
        }
        public async Task<UserGetProfileDTO> GetUserProfile()
        {
            Account account = await _userGlobalServices.GerUser();
            UserGetProfileDTO userGetProfileDTO = new UserGetProfileDTO
            {
                Name = account.Name,    
                UserName = account.Name,    
                Email = account.Email,
                Phone = account.PhoneNumber
            };

            return userGetProfileDTO;   
        }

        public async  Task<string> UpdateEmail(UserUpdateEmailDTO userUpdateEmail)
        {
            Account account = await _userGlobalServices.GerUser(); 

            if (await _userManager.FindByEmailAsync(userUpdateEmail.Email) is not null)
                return "Email is already registerd!";


            var token = await _userManager.GenerateChangeEmailTokenAsync(account, userUpdateEmail.Email);

            var result = await _userManager.ChangeEmailAsync(account, userUpdateEmail.Email, token);

            if (result.Succeeded)
                return "changed";

            else
                throw new Exception(result.Errors.ToString());
        }

        public async Task<string> UpdateName(UserUpdateNameDTO userUpdateName)
        {
            Account account = await _userGlobalServices.GerUser();
            account.Name = userUpdateName.Name; 
            await _userProfileRepository.UpdateUser(account);
            return "changed";
        }

        public async Task<string> UpdatePhone(UserUpdatePhoneDTO userUpdatePhone)
        {
            Account account = await _userGlobalServices.GerUser();
            List<Account>? accounts = await _userManager.Users.Where(u => u.PhoneNumber == userUpdatePhone.Phone).ToListAsync();

            if (accounts.Count() > 0)
                return "PhoneNumber is already registerd!";

            var token = await _userManager.GenerateChangePhoneNumberTokenAsync(account, userUpdatePhone.Phone);

            var result = await _userManager.ChangePhoneNumberAsync(account, userUpdatePhone.Phone, token);

            if (result.Succeeded)
                return "changed";

            else
                throw new Exception(result.Errors.ToString());
        }

    }
}
