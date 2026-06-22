using Microsoft.AspNetCore.Identity;
using SafeDose.Application.DTOs.Admin;
using SafeDose.Application.UseCases.Admin.Auth;
using SafeDose.Domain.Entities;

namespace SafeDose.Application.UseCases.Admin.Accounts;

public class UpdateAdminUseCase
{
    private readonly UserManager<Account> _userManager;
    public UpdateAdminUseCase(UserManager<Account> userManager) => _userManager = userManager;

    public async Task<bool> ExecuteAsync(string accountId, UpdateAdminDto dto)
    {
        if (!AdminLoginUseCase.AllowedRoles.Contains(dto.Role))
            throw new ArgumentException($"Role must be one of: {string.Join(", ", AdminLoginUseCase.AllowedRoles)}");

        var account = await _userManager.FindByIdAsync(accountId);
        if (account == null || account.IsDeleted) return false;

        if (!string.Equals(account.Email, dto.Email, StringComparison.OrdinalIgnoreCase)
            && await _userManager.FindByEmailAsync(dto.Email) is not null)
            throw new InvalidOperationException("Email already in use");

        account.Name     = dto.Name.Trim();
        account.Email    = dto.Email.Trim();
        account.UserName = dto.Email.Trim();

        var updateResult = await _userManager.UpdateAsync(account);
        if (!updateResult.Succeeded)
            throw new InvalidOperationException(string.Join(", ", updateResult.Errors.Select(e => e.Description)));

        // Replace role assignments wholesale (admin can change SuperAdmin <-> Admin).
        var currentRoles = await _userManager.GetRolesAsync(account);
        var toRemove = currentRoles.Intersect(AdminLoginUseCase.AllowedRoles).ToArray();
        if (toRemove.Length > 0)
            await _userManager.RemoveFromRolesAsync(account, toRemove);
        await _userManager.AddToRoleAsync(account, dto.Role);

        if (!string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            if (dto.NewPassword.Length < 8)
                throw new ArgumentException("Password must be at least 8 characters");
            var token = await _userManager.GeneratePasswordResetTokenAsync(account);
            var resetResult = await _userManager.ResetPasswordAsync(account, token, dto.NewPassword);
            if (!resetResult.Succeeded)
                throw new InvalidOperationException(string.Join(", ", resetResult.Errors.Select(e => e.Description)));
        }

        return true;
    }
}
