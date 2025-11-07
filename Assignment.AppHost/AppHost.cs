using Aspire.Hosting;
using Microsoft.Extensions.Hosting;
// Create the distributed application builder.
var builder = DistributedApplication.CreateBuilder(args);
// Add SQL Server database service.
var sqlServerDb = builder.AddSqlServer("theserver")
    .WithImage("mssql/server:2022-latest")
    .AddDatabase("sqldata");

// Register backend (API) and frontend (Blazor) projects.
var api = builder.AddProject<Projects.ObituaryMvcApi>("backend")
    .WithReference(sqlServerDb) // Link the SQL Server database to the API project.
    .WaitFor(sqlServerDb); 
// Link the frontend to the backend API.
builder.AddProject<Projects.ObituaryBlazorWasm>("frontend")
    .WithReference(api)
    .WaitFor(api);

// Build and run the distributed application.
var app = builder.Build();
app.Run();
