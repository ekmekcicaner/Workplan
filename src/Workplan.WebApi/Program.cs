using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Workplan.Application;
using Workplan.Application.Interfaces;
using Workplan.Infrastructure;
using Workplan.Infrastructure.Identity;
using Workplan.Infrastructure.Persistence;
using Workplan.Infrastructure.Persistence.Seed;
using Workplan.SharedKernel.Common;
using Workplan.WebApi.Endpoints;
using Workplan.WebApi.Middlewares;
using Workplan.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

const string ClientCorsPolicy = "WorkplanClient";
builder.Services.AddCors(options =>
{
    options.AddPolicy(ClientCorsPolicy, policy =>
        policy.WithOrigins("https://localhost:7193", "http://localhost:5276", "http://localhost:5277")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, HttpContextCurrentUserService>();

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
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapProjectEndpoints();
app.MapCrewRegionEndpoints();
app.MapLocationEndpoints();
app.MapWorkItemTypeEndpoints();
app.MapCrewEndpoints();
app.MapDailyPlanEndpoints();
app.MapNotificationEndpoints();
app.MapUserEndpoints();

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
