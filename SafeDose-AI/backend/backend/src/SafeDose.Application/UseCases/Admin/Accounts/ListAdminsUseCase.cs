using SafeDose.Application.DTOs.Admin;
using SafeDose.Application.Interfaces.Admin;
using SafeDose.Application.UseCases.Admin.Auth;

namespace SafeDose.Application.UseCases.Admin.Accounts;

public class ListAdminsUseCase
{
    private readonly IAdminAccountRepository _repo;
    public ListAdminsUseCase(IAdminAccountRepository repo) => _repo = repo;

    public async Task<AdminListResponseDto> ExecuteAsync(int page, int pageSize)
    {
        var (items, total) = await _repo.ListByRolesAsync(AdminLoginUseCase.AllowedRoles, page, pageSize);

        // Hydrate role names per row. Small N (admins, dozens at most) so per-row is fine.
        var rows = new List<AdminListItemDto>(items.Count);
        foreach (var a in items)
        {
            var roles = await _repo.GetRoleNamesAsync(a.Id);
            rows.Add(new AdminListItemDto(
                Id:       a.Id,
                Name:     a.Name,
                Email:    a.Email ?? string.Empty,
                Roles:    roles,
                IsActive: a.AccountStatus == 1 && !a.IsDeleted,
                AddedAt:  a.CreatedAt
            ));
        }

        var totalPages = pageSize == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
        return new AdminListResponseDto(rows, page, pageSize, total, totalPages);
    }
}
