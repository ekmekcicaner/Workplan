using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Globalization;
using Workplan.Client;
using Workplan.Client.Auth;
using Workplan.Client.Services;

var culture = CultureInfo.GetCultureInfo("tr-TR");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<ConfirmService>();
builder.Services.AddScoped<ConnectivityService>();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());
builder.Services.AddAuthorizationCore();

builder.Services.AddTransient<AuthorizationMessageHandler>();

// WASM runs in the browser, so it must be told the API's host-published URL via static
// config. wwwroot/appsettings.json ships the local dev default; the Docker build swaps
// in wwwroot/appsettings.Docker.json before publish (see src/Workplan.Client/Dockerfile).
var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("'ApiBaseUrl' konfigürasyonda bulunamadı.");
builder.Services.AddHttpClient("Api", client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<AuthorizationMessageHandler>();
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api"));

builder.Services.AddScoped<AuthApiService>();
builder.Services.AddScoped<ProjectsApiService>();
builder.Services.AddScoped<CrewRegionsApiService>();
builder.Services.AddScoped<LocationsApiService>();
builder.Services.AddScoped<CrewTypesApiService>();
builder.Services.AddScoped<WorkItemTypesApiService>();
builder.Services.AddScoped<DailyPlansApiService>();
builder.Services.AddScoped<NotificationsApiService>();
builder.Services.AddScoped<UsersApiService>();
builder.Services.AddScoped<ReportsApiService>();

await builder.Build().RunAsync();
