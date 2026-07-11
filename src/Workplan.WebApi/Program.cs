using System.Globalization;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using Serilog;
using Workplan.Application;
using Workplan.Application.Interfaces;
using Workplan.Infrastructure;
using Workplan.Infrastructure.Identity;
using Workplan.Infrastructure.Persistence;
using Workplan.Infrastructure.Persistence.Seed;
using Workplan.SharedKernel.Common;
using Workplan.WebApi.Configuration;
using Workplan.WebApi.Endpoints;
using Workplan.WebApi.Middlewares;
using Workplan.WebApi.Services;

const string ServiceName = "Workplan.WebApi";

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfig) => loggerConfig
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(ServiceName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter());

builder.Services.AddOptions<ApiRateLimitOptions>()
    .Bind(builder.Configuration.GetSection(ApiRateLimitOptions.SectionName))
    .Validate(settings => settings.TokenLimit > 0,
        "RateLimiting:TokenLimit sıfırdan büyük olmalıdır.")
    .Validate(settings => settings.TokensPerPeriod > 0,
        "RateLimiting:TokensPerPeriod sıfırdan büyük olmalıdır.")
    .Validate(settings => settings.ReplenishmentPeriodSeconds > 0,
        "RateLimiting:ReplenishmentPeriodSeconds sıfırdan büyük olmalıdır.")
    .Validate(settings => settings.QueueLimit >= 0,
        "RateLimiting:QueueLimit negatif olamaz.")
    .ValidateOnStart();

builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.OnRejected = async (context, cancellationToken) =>
        {
            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                context.HttpContext.Response.Headers.RetryAfter =
                    Math.Ceiling(retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
            }

            var response = ApiResponse<object?>.CreateFailure(
                new ApiError("rate_limited", "Çok fazla istek gönderdiniz. Lütfen kısa bir süre sonra tekrar deneyin."));
            await context.HttpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        };

        options.AddPolicy(ApiRateLimitOptions.PolicyName, httpContext =>
        {
            var settings = httpContext.RequestServices
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<ApiRateLimitOptions>>()
                .Value;
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var clientKey = userId is not null
                ? $"user:{userId}"
                : $"ip:{httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";

            return RateLimitPartition.GetTokenBucketLimiter(clientKey, _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = settings.TokenLimit,
                TokensPerPeriod = settings.TokensPerPeriod,
                ReplenishmentPeriod = TimeSpan.FromSeconds(settings.ReplenishmentPeriodSeconds),
                QueueLimit = settings.QueueLimit,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            });
        });
    }
);

builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
// UseExceptionHandler() bir ExceptionHandler/ExceptionHandlingPath ya da IProblemDetailsService
// fallback'i şart koşuyor; GlobalExceptionHandler tüm exception'ları kendi ApiResponse zarfıyla
// ele aldığı için ProblemDetails gövdesi hiç üretilmez, ama servis kaydı yine de gerekli.
builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

const string ClientCorsPolicy = "WorkplanClient";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? throw new InvalidOperationException("'Cors:AllowedOrigins' bulunamadı.");

builder.Services.AddCors(options =>
{
    options.AddPolicy(ClientCorsPolicy, policy =>
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, HttpContextCurrentUserService>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(name: "postgres", tags: ["ready"]);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages(async statusCodeContext =>
{
    var httpContext = statusCodeContext.HttpContext;

    var (code, message) = httpContext.Response.StatusCode switch
    {
        StatusCodes.Status400BadRequest => ("bad_request", "Geçersiz istek."),
        StatusCodes.Status401Unauthorized => ("unauthorized", "Bu işlem için giriş yapmanız gerekiyor."),
        StatusCodes.Status403Forbidden => ("forbidden", "Bu işlem için yetkiniz yok."),
        StatusCodes.Status404NotFound => ("not_found", "Kaynak bulunamadı."),
        StatusCodes.Status405MethodNotAllowed => ("method_not_allowed", "Bu HTTP metoduna izin verilmiyor."),
        _ => ("error", "Beklenmeyen bir hata oluştu.")
    };

    var response = ApiResponse<object?>.CreateFailure(new ApiError(code, message));
    await httpContext.Response.WriteAsJsonAsync(response);
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(ClientCorsPolicy);
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

var api = app.MapGroup("/api")
    .RequireRateLimiting(ApiRateLimitOptions.PolicyName);

api.MapAuthEndpoints();
api.MapProjectEndpoints();
api.MapCrewRegionEndpoints();
api.MapLocationEndpoints();
api.MapWorkItemTypeEndpoints();
api.MapCrewTypeEndpoints();
api.MapDailyPlanEndpoints();
api.MapReportEndpoints();
api.MapNotificationEndpoints();
api.MapUserEndpoints();

// Liveness: proses ayakta mı — bağımlılık kontrolü yok, her zaman hızlı döner.
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });

// Readiness: trafiği kabul etmeye hazır mı — "ready" etiketli kontroller (örn. Postgres) çalıştırılır.
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();

    await IdentitySeeder.SeedAsync(scope.ServiceProvider, app.Configuration);

    if (app.Configuration.GetValue<bool>("SeedDemoData"))
        await DemoDataSeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();

public partial class Program;
