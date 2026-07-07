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

// The client is launched on local IDE ports (5276/7193) or Docker's app port
// (5277), and must call the API on the matching host port.
// WASM runs in the browser, so launch-profile env vars never reach it; map the
// client's own origin to the API origin, falling back to config for production.
var clientBaseUri = new Uri(builder.HostEnvironment.BaseAddress);
var apiBaseUrl = (IsLoopbackHost(clientBaseUri.Host), clientBaseUri.Scheme, clientBaseUri.Port) switch
{
    (true, "https", _) => "https://localhost:7272",
    (true, "http", 5277) => "http://localhost:5292",
    (true, "http", _) => "http://localhost:5291",
    _ => builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress
};
builder.Configuration["ApiBaseUrl"] = apiBaseUrl;
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

static bool IsLoopbackHost(string host) =>
    host is "localhost" or "127.0.0.1" or "::1";
