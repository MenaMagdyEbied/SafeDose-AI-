using SafeDose.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeDose.Application.Auth.ServicesInterfaces
{
    public interface IUserGlobalServices
    {
        Task<Account> GerUser();
    }
}
