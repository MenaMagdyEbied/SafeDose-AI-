using Microsoft.AspNetCore.Identity;
using SafeDose.Application.DTOs.Admin;
using SafeDose.Application.UseCases.Admin.Auth;
using SafeDose.Domain.Entities;

namespace SafeDose.Application.UseCases.Admin.Accounts;

// Goes through UserManager so password hashing, normalization, and unique-email
// validation all stay consistent with regular registrations.
public class CreateAdminUseCase
{
    private readonly UserManager<Account> _userManager;
    public CreateAdminUseCase(UserManager<Account> userManager) => _userManager = userManager;

    public async Task<string> ExecuteAsync(CreateAdminDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            throw new ArgumentException("Email and password are required");
        if (dto.Password.Length < 8)
            throw new ArgumentException("Password must be at least 8 characters");
        if (!AdminLoginUseCase.AllowedRoles.Contains(dto.Role))
            throw new ArgumentException($"Role must be one of: {string.Join(", ", AdminLoginUseCase.AllowedRoles)}");

        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
            throw new InvalidOperationException("Email already in use");

        var account = new Account
        {
            Name           = dto.Name.Trim(),
            Email          = dto.Email.Trim(),
            UserName       = dto.Email.Trim(),
            EmailConfirmed = true,
            AccountStatus  = 1,
            CreatedAt      = DateTime.UtcNow,
        };

        var result = await _userManager.CreateAsync(account, dto.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(account, dto.Role);
        return account.Id;
    }
}
