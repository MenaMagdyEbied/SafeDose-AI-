using SafeDose.Application.UserProfile.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeDose.Application.UserProfile.ServicesInterface
{
    public interface IUserProfileServices
    {
        Task<UserGetProfileDTO> GetUserProfile();
        Task<string> UpdateName(UserUpdateNameDTO userUpdateName);
        Task<string> UpdateEmail(UserUpdateEmailDTO userUpdateEmail);
        Task<string> UpdatePhone(UserUpdatePhoneDTO  userUpdatePhone);

    }
}
