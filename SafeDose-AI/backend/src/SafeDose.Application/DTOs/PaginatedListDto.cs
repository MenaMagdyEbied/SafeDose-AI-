namespace SafeDose.Application.DTOs;

// Generic pagination envelope so Doaa's frontend always knows
// how many total items exist + can render "Load more" or numbered pages.
public record PaginatedListDto<T>(
    int Total,
    int Limit,
    int Offset,
    IReadOnlyList<T> Items
);
