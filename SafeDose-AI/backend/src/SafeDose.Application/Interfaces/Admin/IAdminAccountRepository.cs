using SafeDose.Domain.Entities;

namespace SafeDose.Application.Interfaces.Admin;

// Admin-only queries against the Accounts table that don't belong in IAuthService.
// CRUD goes through UserManager<Account> in the use case so password hashing
// and Identity validation stay correct.
public interface IAdminAccountRepository
{
    Task<(IReadOnlyList<Account> Items, int Total)> ListByRolesAsync(
        string[] roleNames, int page, int pageSize);

    Task<IReadOnlyList<string>> GetRoleNamesAsync(string accountId);
}
