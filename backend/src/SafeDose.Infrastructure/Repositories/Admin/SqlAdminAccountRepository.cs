using Microsoft.EntityFrameworkCore;
using SafeDose.Application.Interfaces.Admin;
using SafeDose.Domain.ApplicationDbContext;
using SafeDose.Domain.Entities;

namespace SafeDose.Infrastructure.Repositories.Admin;

public class SqlAdminAccountRepository : IAdminAccountRepository
{
    private readonly AppDbContext _db;
    public SqlAdminAccountRepository(AppDbContext db) => _db = db;

    public async Task<(IReadOnlyList<Account> Items, int Total)> ListByRolesAsync(
        string[] roleNames, int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 200) pageSize = 20;

        var userIds = from r in _db.Roles
                      join ur in _db.UserRoles on r.Id equals ur.RoleId
                      where roleNames.Contains(r.Name)
                      select ur.UserId;

        var baseQuery = _db.Accounts.Where(a => !a.IsDeleted && userIds.Distinct().Contains(a.Id));
        var total = await baseQuery.CountAsync();
        var items = await baseQuery
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<IReadOnlyList<string>> GetRoleNamesAsync(string accountId)
    {
        var q = from ur in _db.UserRoles
                join r in _db.Roles on ur.RoleId equals r.Id
                where ur.UserId == accountId && r.Name != null
                select r.Name!;
        return await q.ToListAsync();
    }
}
