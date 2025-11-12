using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ObituaryMvcApi.Data;
using ObituaryMvcApi.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.AI;
using Azure.AI.OpenAI;
using Azure;
using Azure.Identity;


var builder = WebApplication.CreateBuilder(args);

var endpoint = builder.Configuration["AI:Endpoint"];
var apiKey = builder.Configuration["AI:ApiKey"];
var model = builder.Configuration["AI:ModelName"];

builder.Services.AddChatClient(services =>
  new ChatClientBuilder(
    (
      !string.IsNullOrEmpty(apiKey)
        ? new AzureOpenAIClient(new Uri(endpoint!), new AzureKeyCredential(apiKey))
        : new AzureOpenAIClient(new Uri(endpoint!), new DefaultAzureCredential())
    ).GetChatClient(model).AsIChatClient()
  )
  .UseFunctionInvocation()
  .Build());

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger to include controller-based API endpoints only
builder.Services.AddSwaggerGen(c =>
{
    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        var actionDescriptor = apiDesc.ActionDescriptor;
        return actionDescriptor?.RouteValues != null && actionDescriptor.RouteValues.ContainsKey("controller");
    });
    
    // Use full type names to avoid schema ID conflicts
    c.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
});

// Configure JSON serializer for consistent camelCase output
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.SerializerOptions.WriteIndented = true;
});

// Database connection (uses service defaults)
builder.AddSqlServerDbContext<ApplicationDbContext>("sqldata");

// Authorization middleware
builder.Services.AddAuthorization(options =>
{
    // ApiPolicy requires an authenticated user (supports JWT Bearer tokens)
    options.AddPolicy("ApiPolicy", policy =>
        policy.RequireAuthenticatedUser()
              .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme));
});
// catch database errors
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configure JWT authentication for API clients
var jwtSection  = builder.Configuration.GetSection("Jwt");
var jwtKey      = jwtSection.GetValue<string>("Key") ?? string.Empty;
var jwtIssuer   = jwtSection.GetValue<string>("Issuer");
var jwtAudience = jwtSection.GetValue<string>("Audience");
var keyBytes    = Encoding.UTF8.GetBytes(jwtKey);

// This jwt code configures the API to use JWT Bearer tokens for authentication on requests.
// Completely different role from the code in JwtTokenService.cs that creates the tokens.
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata      = true;
        options.SaveToken                 = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtIssuer,
            ValidAudience            = jwtAudience,
            IssuerSigningKey         = new SymmetricSecurityKey(keyBytes)
        };
    });

// Add Identity with roles
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Stores.MaxLengthForKeys = 128;          // Key/ID length

    // Password settings
    options.Password.RequireDigit           = true;          // at least one digit
    options.Password.RequireLowercase       = true;          // at least one lowercase letter
    options.Password.RequireNonAlphanumeric = true;          // at least one special character required (examples: ! @ # $ % ^ & * ( ) - _ = + . , and also spaces)
    options.Password.RequireUppercase       = true;          // at least one uppercase letter
    options.Password.RequiredLength         = 8;             // minimum length
    options.Password.RequiredUniqueChars    = 1;             // at least one unique character

    // Sign-in settings - require confirmed account for login
    options.SignIn.RequireConfirmedAccount = false; // Set to false to allow immediate login after registration
})
.AddEntityFrameworkStores<ApplicationDbContext>()   // Store users and roles in the database
.AddRoles<IdentityRole>()                           // Add role management RoleManager<IdentityRole>, UserManager<IdentityUser>
.AddDefaultUI()                                     // Add the default UI (Razor pages for login, register, etc.)
.AddDefaultTokenProviders()                         // Add token providers for password reset, email confirmation, etc.
.AddApiEndpoints();                                 // Add API endpoints support

builder.Services.AddControllersWithViews();

// Register JWT token generator service
builder.Services.AddScoped<ObituaryMvcApi.Services.IJwtTokenService, ObituaryMvcApi.Services.JwtTokenService>();

// Add CORS
builder.Services.AddCors(o => o.AddPolicy("AllowAllPolicy", builder =>
{
    builder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

// Use CORS - must be before UseRouting
app.UseCors("AllowAllPolicy");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Static assets mapping (kept in place)
app.MapStaticAssets();
// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

// Seed identity data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ApplicationDbContext>();

    context.Database.Migrate();

    var userMgr = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleMgr = services.GetRequiredService<RoleManager<IdentityRole>>();

    IdentitySeedData.Initialize(context, userMgr, roleMgr).Wait();
}

app.MapIdentityApi<IdentityUser>();

// This block of code makes Swagger available in deployed environments
// Enable Swagger in non-development via configuration and protect it so only Admins can access
var enableSwagger = builder.Configuration.GetValue<bool>("EnableSwagger", false);
if (app.Environment.IsDevelopment() || enableSwagger)
{
    // Protect the /swagger endpoints so anonymous or non-admin users cannot view them in production
    // But allow unrestricted access in development
    if (!app.Environment.IsDevelopment())
    {
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase) ||
                context.Request.Path.StartsWithSegments("/openapi", StringComparison.OrdinalIgnoreCase))
            {
                // If user is not authenticated, this will redirect to login if using cookie auth
                if (!(context.User?.Identity?.IsAuthenticated ?? false))
                {
                    // Redirect to the Identity login page with return url so the user can authenticate
                    var returnUrl = Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
                    context.Response.Redirect($"/Identity/Account/Login?ReturnUrl={returnUrl}");
                    return;
                }

                // If authenticated but not in Admin role, forbid
                if (!context.User.IsInRole("Admin"))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return;
                }
            }
            await next();
        });
    }

    app.UseSwagger(options =>
    {
        options.RouteTemplate = "swagger/{documentName}/swagger.json";
    });
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MCP Server v1");
        options.RoutePrefix = "swagger";
    });
}

// Controller-based APIs: map attribute-routed controllers (our ObituariesApiController)
app.MapControllers();

app.Run();
