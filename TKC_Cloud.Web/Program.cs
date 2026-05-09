using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using TKC_Cloud.Web;
using TKC_Cloud.Web.Services;
using TKC_Cloud.Web.Services.ServicesHealthCheck;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = true;
    config.SnackbarConfiguration.NewestOnTop = true;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 8000;
    config.SnackbarConfiguration.HideTransitionDuration = 200;
    config.SnackbarConfiguration.ShowTransitionDuration = 200;
});
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, AuthProvider>();
builder.Services.AddScoped<TokenHandler>();

// Health Checks Service
builder.Services.AddHttpClient();
builder.Services.AddScoped<GlobalErrorService>();

//builder.Services.AddScoped<IServiceHealthCheck, ClouldServiceHealthCheck>();

//builder.Services.AddScoped<HealthCheckService>();

builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri("http://localhost:5166/");
})
.AddHttpMessageHandler<TokenHandler>();

builder.Services.AddHttpClient("Auth", client =>
{
    client.BaseAddress = new Uri("http://localhost:5166/"); 
});

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

await builder.Build().RunAsync();
