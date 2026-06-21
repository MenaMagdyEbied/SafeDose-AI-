namespace SafeDose.Application.DTOs.Admin;

public record AdminListItemDto(
    string Id,
    string Name,
    string Email,
    IReadOnlyList<string> Roles,
    bool IsActive,
    DateTime AddedAt
);

public record AdminListResponseDto(
    IReadOnlyList<AdminListItemDto> Items,
    int Page,
    int PageSize,
    int Total,
    int TotalPages
);

public record CreateAdminDto(
    string Name,
    string Email,
    string Password,
    string Role           // "SuperAdmin" or "Admin"
);

public record UpdateAdminDto(
    string  Name,
    string  Email,
    string  Role,
    string? NewPassword
);

public record ToggleAdminStatusDto(bool IsActive);
