namespace BWell.Loyalty.Application.DTOs.Analytics;

public record DashboardOverviewDto(
    decimal TotalRevenueEgp,
    int TotalSubscriptions,
    int ActivePrograms,
    decimal AverageValueEgp,
    IReadOnlyList<ProgramPerformanceDto> ProgramPerformance,
    ChronicDiseaseStatsDto ChronicDiseaseStats,
    IReadOnlyList<GeographicPointDto> GeographicDistribution);

public record ProgramPerformanceDto(
    Guid ProgramId,
    string Name,
    string Icon,
    string Color,
    int MemberCount,
    decimal RevenueEgp,
    decimal SharePercent);

public record ChronicDiseaseStatsDto(
    decimal PercentWithConditions,
    int MembersWithConditions,
    int TotalMembers,
    IReadOnlyList<ChronicDiseaseBreakdownDto> Breakdown);

public record ChronicDiseaseBreakdownDto(
    string Name,
    int MemberCount,
    decimal Percent);

public record GeographicPointDto(
    Guid GovernorateId,
    string Name,
    string NameAr,
    double Latitude,
    double Longitude,
    int MemberCount,
    string DensityLevel);

public record ChronicDiseaseLookupDto(Guid Id, string Name, string Code);
public record MedicalServiceLookupDto(Guid Id, string Name, string? Description);
public record GovernorateLookupDto(Guid Id, string Name, string NameAr);
