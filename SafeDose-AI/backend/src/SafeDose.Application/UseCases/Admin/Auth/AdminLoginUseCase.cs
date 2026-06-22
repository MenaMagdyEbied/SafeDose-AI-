using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SafeDose.Application.DTOs.Admin;
using SafeDose.Domain.Entities;
using SafeDose.Shared.helper;

namespace SafeDose.Application.UseCases.Admin.Auth;

// Mirrors AuthService.GetTokenAsync + CreateJwtToken but:
//  - Looks up by email instead of username (admins log in with email on the dashboard).
//  - Rejects accounts that are not in SuperAdmin or Admin role.
//  - Uses the same JWT signing options so the bearer works against the same middleware.
public class AdminLoginUseCase
{
    public static readonly string[] AllowedRoles = { "SuperAdmin", "Admin" };

    private readonly UserManager<Account> _userManager;
    private readonly JWT _jwt;

    public AdminLoginUseCase(UserManager<Account> userManager, IOptions<JWT> jwt)
    {
        _userManager = userManager;
        _jwt = jwt.Value;
    }

    public async Task<AdminLoginResponseDto?> ExecuteAsync(AdminLoginRequestDto req)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            return null;

        var user = await _userManager.FindByEmailAsync(req.Email.Trim());
        if (user == null || user.IsDeleted) return null;
        if (!await _userManager.CheckPasswordAsync(user, req.Password)) return null;

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Any(r => AllowedRoles.Contains(r))) return null;

        var jwt = BuildJwt(user, roles);
        var token = new JwtSecurityTokenHandler().WriteToken(jwt);

        return new AdminLoginResponseDto(
            Token:     token,
            ExpiresAt: jwt.ValidTo,
            AccountId: user.Id,
            Email:     user.Email ?? string.Empty,
            Name:      user.Name,
            Roles:     roles.ToList()
        );
    }

    private JwtSecurityToken BuildJwt(Account user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,   user.Id),
            new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier,     user.Id),
        };
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        return new JwtSecurityToken(
            issuer:             _jwt.Issuer,
            audience:           _jwt.Audience,
            claims:             claims,
            expires:            DateTime.Now.AddDays(_jwt.DurationInDays),
            signingCredentials: creds);
    }
}
