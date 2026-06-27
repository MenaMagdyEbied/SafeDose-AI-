namespace SafeDose.Application.DTOs.Admin;

public record AdminLoginRequestDto(string Email, string Password);

public record AdminLoginResponseDto(
    string Token,
    DateTime ExpiresAt,
    string AccountId,
    string Email,
    string Name,
    IReadOnlyList<string> Roles
);
