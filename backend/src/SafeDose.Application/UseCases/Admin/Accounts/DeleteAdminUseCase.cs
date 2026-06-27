using Microsoft.AspNetCore.Identity;
using SafeDose.Application.Interfaces.Admin;
using SafeDose.Domain.Entities;

namespace SafeDose.Application.UseCases.Admin.Accounts;

// Soft-deletes the admin (sets IsDeleted=true). Refuses to delete the last SuperAdmin
// so the organisation can never lock itself out of the dashboard.
public class DeleteAdminUseCase
{
    private readonly UserManager<Account>    _userManager;
    private readonly IAdminStatsRepository   _stats;

    public DeleteAdminUseCase(UserManager<Account> userManager, IAdminStatsRepository stats)
    {
        _userManager = userManager;
        _stats       = stats;
    }

    public async Task<bool> ExecuteAsync(string accountId)
    {
        var account = await _userManager.FindByIdAsync(accountId);
        if (account == null || account.IsDeleted) return false;

        var roles = await _userManager.GetRolesAsync(account);
        if (roles.Contains("SuperAdmin"))
        {
            var superAdminCount = await _stats.CountAccountsByRoleAsync("SuperAdmin");
            if (superAdminCount <= 1)
                throw new InvalidOperationException("Cannot delete the last SuperAdmin account");
        }

        account.IsDeleted = true;
        account.AccountStatus = 0;
        var result = await _userManager.UpdateAsync(account);
        return result.Succeeded;
    }
}
