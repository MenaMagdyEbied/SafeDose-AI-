using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SafeDose.Application.Auth.ServicesInterfaces;
using SafeDose.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeDose.Infrastructure.Auth
{
    public class UserGlobalServices : IUserGlobalServices
    {
        private readonly UserManager<Account> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserGlobalServices(IHttpContextAccessor httpContextAccessor, UserManager<Account> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task<Account> GetUser()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) throw new Exception("Not found this User");
            string? userName = _userManager.GetUserId(user);
            if (userName == null) throw new Exception("Not found this User");
            Account? userLogin = await _userManager.FindByNameAsync(userName);
            if (userLogin == null) throw new Exception("Not found this User");

            return userLogin;
        }
    }
}
