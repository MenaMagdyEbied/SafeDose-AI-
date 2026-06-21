namespace SafeDose.Application.DTOs.Admin;

public record KpiCardDto(decimal Value, double TrendPercent, string Currency);

public record AdminDashboardKpisDto(
    KpiCardDto YearlyRevenue,
    KpiCardDto MonthlyRevenue,
    int        ActiveUsers,
    double     ActiveUsersTrendPercent,
    int        TotalUsers,
    double     TotalUsersTrendPercent
);

public record RevenuePointDto(int Year, int Month, string MonthLabelArabic, decimal Total);

public record AdminRevenueChartDto(
    string  Period,                              // "monthly" or "yearly"
    decimal CurrentTotal,
    double  TrendPercent,
    IReadOnlyList<RevenuePointDto> Points
);

public record GenderDistributionDto(int Male, int Female, int Other, int Total)
{
    public double MalePercent   => Total == 0 ? 0 : Math.Round((double)Male   * 100 / Total, 1);
    public double FemalePercent => Total == 0 ? 0 : Math.Round((double)Female * 100 / Total, 1);
}

public record TreatmentCardsDto(int Issued, int Active, int Expired);

public record TeamBreakdownDto(int Admins, int Caregivers, int Reviewers);

public record FreeVsPaidDto(int Free, int Paid)
{
    public double ConversionPercent =>
        (Free + Paid) == 0 ? 0 : Math.Round((double)Paid * 100 / (Free + Paid), 1);
}

public record ActivityFeedItemDto(string Type, string TitleArabic, DateTime AtUtc);
