using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Workplan.Client;
using Workplan.Client.Auth;
using Workplan.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<ConfirmService>();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());
builder.Services.AddAuthorizationCore();

builder.Services.AddTransient<AuthorizationMessageHandler>();

// The client is launched on either the http (5276) or https (7193) profile and
// must call the API on the matching scheme, otherwise it hits a port that isn't
// listening (http profile) or a mixed-content wall (https page -> http API).
// WASM runs in the browser, so launch-profile env vars never reach it; map the
// client's own origin to the API origin, falling back to config for production.
var clientBaseUri = new Uri(builder.HostEnvironment.BaseAddress);
var apiBaseUrl = (clientBaseUri.Host, clientBaseUri.Scheme) switch
{
    ("localhost", "https") => "https://localhost:7272",
    ("localhost", "http") => "http://localhost:5291",
    _ => builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress
};
builder.Services.AddHttpClient("Api", client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<AuthorizationMessageHandler>();
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api"));

builder.Services.AddScoped<AuthApiService>();
builder.Services.AddScoped<ProjectsApiService>();
builder.Services.AddScoped<CrewRegionsApiService>();
builder.Services.AddScoped<LocationsApiService>();
builder.Services.AddScoped<CrewsApiService>();
builder.Services.AddScoped<WorkItemTypesApiService>();
builder.Services.AddScoped<DailyPlansApiService>();
builder.Services.AddScoped<NotificationsApiService>();
builder.Services.AddScoped<UsersApiService>();

await builder.Build().RunAsync();
