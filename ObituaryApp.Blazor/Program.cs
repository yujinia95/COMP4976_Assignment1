using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ObituaryApp.Blazor;
using ObituaryApp.Blazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient to point to your MVC API
// You can configure this in appsettings.json or environment variables for different environments
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7001/";

builder.Services.AddScoped(sp => new HttpClient { 
    BaseAddress = new Uri(apiBaseUrl)
});

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IObituaryService, ObituaryService>();

await builder.Build().RunAsync();
