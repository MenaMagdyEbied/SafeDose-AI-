using SafeDose.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeDose.Application.UserProfile.RepositoryInterface
{
    public interface IUserProfileRepository
    {
        Task<bool> UpdateUser(Account account);
    }
}
