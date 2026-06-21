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
            if (user == null) throw new Exception("لم يتم العثور على هذا المستخدم");
            string? userId = _userManager.GetUserId(user);
            if (userId == null) throw new Exception("لم يتم العثور على هذا المستخدم");
            Account? userLogin = await _userManager.FindByIdAsync(userId);
            if (userLogin == null) throw new Exception("لم يتم العثور على هذا المستخدم");

            return userLogin;
        }
    }
}
