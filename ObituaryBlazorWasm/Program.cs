// Import Blazor WebAssembly and related namespaces
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ObituaryBlazorWasm;

// Create a new WebAssembly host builder instance
// This sets up the Blazor WebAssembly environment, configuration, and DI container.
var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Register the root component (<App />) and specify where it should be rendered in index.html
builder.RootComponents.Add<App>("#app");

// Register the <HeadOutlet /> component, which allows Blazor to modify the <head> section dynamically (e.g., titles, meta tags)
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure dependency injection for HttpClient
// By default, this points to the same base address as the Blazor client app itself (e.g., https://localhost:7171/)
// Note: if your API runs on a different port (e.g., 7070), you’ll need to replace BaseAddress with that URL
// builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }); // default
// var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7070"; // recommended for production use
var apiBaseUrl = "https://localhost:7070"; // hardcoded for development/testing purposes
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });


// Build the host and start the Blazor WebAssembly app
await builder.Build().RunAsync();
