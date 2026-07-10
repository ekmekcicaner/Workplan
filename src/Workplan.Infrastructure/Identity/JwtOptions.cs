using Microsoft.Extensions.Options;

namespace Workplan.Infrastructure.Identity;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SigningKey { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int AccessTokenMinutes { get; init; } = 60;
    public int RefreshTokenDays { get; init; } = 7;
}

public sealed class JwtOptionsValidator : IValidateOptions<JwtOptions>
{
    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.SigningKey))
            return ValidateOptionsResult.Fail("'Jwt:SigningKey' bulunamadı.");
        if (string.IsNullOrWhiteSpace(options.Issuer))
            return ValidateOptionsResult.Fail("'Jwt:Issuer' bulunamadı.");
        if (string.IsNullOrWhiteSpace(options.Audience))
            return ValidateOptionsResult.Fail("'Jwt:Audience' bulunamadı.");
        if (options.AccessTokenMinutes <= 0)
            return ValidateOptionsResult.Fail("'Jwt:AccessTokenMinutes' pozitif olmalı.");
        if (options.RefreshTokenDays <= 0)
            return ValidateOptionsResult.Fail("'Jwt:RefreshTokenDays' pozitif olmalı.");

        return ValidateOptionsResult.Success;
    }
}
