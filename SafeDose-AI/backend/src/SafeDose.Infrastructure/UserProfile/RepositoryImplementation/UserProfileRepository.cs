using Microsoft.AspNetCore.Identity;
using SafeDose.Application.UserProfile.RepositoryInterface;
using SafeDose.Domain.ApplicationDbContext;
using SafeDose.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeDose.Infrastructure.UserProfile.RepositoryImplementation
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Account> _userManager;

        public UserProfileRepository(AppDbContext context , UserManager<Account> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<bool> UpdateUser(Account account)
        {
           await _userManager.UpdateAsync(account);
           await _context.SaveChangesAsync();

           return true;
        }
    }
}
