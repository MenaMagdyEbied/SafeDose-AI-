using Microsoft.AspNetCore.Identity;
using SafeDose.Domain.Entities;

namespace SafeDose.Application.UseCases.Admin.Accounts;

public class ToggleAdminStatusUseCase
{
    private readonly UserManager<Account> _userManager;
    public ToggleAdminStatusUseCase(UserManager<Account> userManager) => _userManager = userManager;

    public async Task<bool> ExecuteAsync(string accountId, bool isActive)
    {
        var account = await _userManager.FindByIdAsync(accountId);
        if (account == null || account.IsDeleted) return false;

        account.AccountStatus = isActive ? (byte)1 : (byte)0;
        var result = await _userManager.UpdateAsync(account);
        return result.Succeeded;
    }
}
