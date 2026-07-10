using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Workplan.Application.Interfaces;
using Workplan.Infrastructure.Identity;
using Workplan.Infrastructure.Messaging;
using Workplan.Infrastructure.Messaging.Outbox;
using Workplan.Infrastructure.Messaging.Webhooks;
using Workplan.Infrastructure.Persistence;
using Workplan.Application.Common.Messaging;

namespace Workplan.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("'Default' bağlantı dizesi bulunamadı.");

        services.AddSingleton<OutboxSaveChangesInterceptor>();
        services.AddDbContext<AppDbContext>((serviceProvider, options) => options
            .UseNpgsql(connectionString)
            .AddInterceptors(serviceProvider.GetRequiredService<OutboxSaveChangesInterceptor>()));
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddOptions<OutboxOptions>()
            .Bind(configuration.GetSection(OutboxOptions.SectionName))
            .Validate(options => options.PollIntervalSeconds > 0, "Outbox:PollIntervalSeconds sıfırdan büyük olmalıdır.")
            .Validate(options => options.BatchSize is > 0 and <= 500, "Outbox:BatchSize 1-500 aralığında olmalıdır.")
            .Validate(options => options.MaxRetryCount > 0, "Outbox:MaxRetryCount sıfırdan büyük olmalıdır.")
            .Validate(options => options.BaseRetryDelaySeconds > 0, "Outbox:BaseRetryDelaySeconds sıfırdan büyük olmalıdır.")
            .Validate(options => options.MaxRetryDelaySeconds >= options.BaseRetryDelaySeconds,
                "Outbox:MaxRetryDelaySeconds, BaseRetryDelaySeconds değerinden küçük olamaz.")
            .ValidateOnStart();

        services.AddOptions<WebhookOptions>()
            .Bind(configuration.GetSection(WebhookOptions.SectionName))
            .Validate(options =>
                    !options.Enabled
                    || Uri.TryCreate(options.Endpoint, UriKind.Absolute, out var endpoint)
                    && endpoint.Scheme is "http" or "https",
                "IntegrationWebhook:Endpoint geçerli bir HTTP(S) adresi olmalıdır.")
            .Validate(options =>
                    !options.Enabled || !string.IsNullOrWhiteSpace(options.SigningSecret),
                "IntegrationWebhook:SigningSecret webhook etkin olduğunda zorunludur.")
            .Validate(options => options.TimeoutSeconds > 0,
                "IntegrationWebhook:TimeoutSeconds sıfırdan büyük olmalıdır.")
            .ValidateOnStart();

        services.AddSingleton(TimeProvider.System);
        services.AddHttpClient<WebhookIntegrationEventPublisher>();
        services.AddScoped<IIntegrationEventPublisher, IntegrationEventPublisher>();
        services.AddScoped<OutboxProcessor>();
        services.AddHostedService<OutboxDispatcher>();

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddSingleton<IValidateOptions<JwtOptions>, JwtOptionsValidator>();
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateOnStart();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearerOptions, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;

                bearerOptions.MapInboundClaims = false;
                bearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorization();

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ITokenService, JwtTokenService>();

        return services;
    }
}
