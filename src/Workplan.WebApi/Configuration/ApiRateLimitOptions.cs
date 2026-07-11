namespace Workplan.WebApi.Configuration;

public sealed class ApiRateLimitOptions
{
    public const string SectionName = "RateLimiting";
    public const string PolicyName = "api-token-bucket";

    public int TokenLimit { get; init; } = 20;
    public int TokensPerPeriod { get; init; } = 5;
    public int ReplenishmentPeriodSeconds { get; init; } = 5;
    public int QueueLimit { get; init; } = 2;
}
