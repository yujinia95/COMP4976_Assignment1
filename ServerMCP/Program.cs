using ServerMCP.Data;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults (telemetry, health checks, etc.)
builder.AddServiceDefaults();

builder.Services.AddMcpServer()
.WithHttpTransport()
.WithToolsFromAssembly();

// Database connection (uses service defaults)
builder.AddSqlServerDbContext<ApplicationDbContext>("sqldata");

var app = builder.Build();

// Map Aspire defaults (health checks, etc.)
app.MapDefaultEndpoints();

// Add MCP middleware
app.MapMcp();

app.Run();
