using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ObivtuaryMvcApi.Data;
using ObivtuaryMvcApi.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure JSON options for minimal APIs
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.SerializerOptions.WriteIndented = true;
});

// Database connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Authorization middleware
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiPolicy", policy =>
        policy.RequireAuthenticatedUser()
              .AddAuthenticationSchemes(IdentityConstants.BearerScheme));
});
// catch database errors
builder.Services.AddDatabaseDeveloperPageExceptionFilter();


// Add Authentication schemes
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.BearerScheme;
    options.DefaultChallengeScheme = IdentityConstants.BearerScheme;
})
    .AddBearerToken(IdentityConstants.BearerScheme);

// Add Identity with roles
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Stores.MaxLengthForKeys = 128; // Key/ID length

    // Password settings
    options.Password.RequireDigit = true; // at least one digit
    options.Password.RequireLowercase = true; // at least one lowercase letter
    options.Password.RequireNonAlphanumeric = true; // at least one special character required (examples: ! @ # $ % ^ & * ( ) - _ = + . , and also spaces)
    options.Password.RequireUppercase = true; // at least one uppercase letter
    options.Password.RequiredLength = 8; // minimum length
    options.Password.RequiredUniqueChars = 1; // at least one unique character
})
.AddEntityFrameworkStores<ApplicationDbContext>() // Store users and roles in the database
.AddRoles<IdentityRole>() // Add role management RoleManager<IdentityRole>, UserManager<IdentityUser>
.AddDefaultUI() // Add the default UI (Razor pages for login, register, etc.)
.AddDefaultTokenProviders() // Add token providers for password reset, email confirmation, etc.
.AddApiEndpoints(); // Add API endpoints support

builder.Services.AddControllersWithViews();

// Add CORS
builder.Services.AddCors(o => o.AddPolicy("AllowAllPolicy", builder => {
    builder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseMigrationsEndPoint();
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }
// else
// {
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
// }

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

// This block of code makes Swagger available in deployeed environments
// Enable Swagger in non-development via configuration and protect it so only Admins can access
var enableSwagger = builder.Configuration.GetValue<bool>("EnableSwagger", false);
if (app.Environment.IsDevelopment() || enableSwagger)
{
    // Protect the /swagger endpoints so anonymous or non-admin users cannot view them in production
    app.Use(async (context, next) =>
    {
        if (context.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase))
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

    app.UseSwagger();
    app.UseSwaggerUI();
}

// Minimal API endpoints for Obituaries
// GET /api/obituaries - Get all obituaries
app.MapGet("/api/obituaries", async (ApplicationDbContext context) =>
{
    var obituaries = await context.Obituaries.ToListAsync();
    return Results.Ok(obituaries);
})
.WithName("GetObituaries")
.WithTags("Obituaries")
.WithOpenApi();

// GET /api/obituaries/{id} - Get specific obituary by ID
app.MapGet("/api/obituaries/{id:int}", async (int id, ApplicationDbContext context) =>
{
    var obituary = await context.Obituaries.FindAsync(id);
    
    if (obituary == null)
    {
        return Results.NotFound($"Obituary with ID {id} not found.");
    }
    
    return Results.Ok(obituary);
})
.WithName("GetObituary")
.WithTags("Obituaries")
.WithOpenApi();

// POST /api/obituaries - Create new obituary (requires authentication)
app.MapPost("/api/obituaries", async (ObituaryCreateDto obituaryDto, ApplicationDbContext context, ClaimsPrincipal user) =>
{
    // Validate date logic
    if (obituaryDto.DateOfDeath < obituaryDto.DateOfBirth)
    {
        return Results.BadRequest("Date of Death cannot be before Date of Birth.");
    }

    if (obituaryDto.DateOfBirth > DateTime.Now)
    {
        return Results.BadRequest("Date of Birth cannot be in the future.");
    }

    if (obituaryDto.DateOfDeath > DateTime.Now)
    {
        return Results.BadRequest("Date of Death cannot be in the future.");
    }

    var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

    var obituary = new Obituary
    {
        FullName = obituaryDto.FullName,
        DateOfBirth = obituaryDto.DateOfBirth,
        DateOfDeath = obituaryDto.DateOfDeath,
        Biography = obituaryDto.Biography,
        Photo = obituaryDto.Photo,
        CreatedByUserId = userId,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    context.Obituaries.Add(obituary);
    await context.SaveChangesAsync();

    return Results.CreatedAtRoute("GetObituary", new { id = obituary.Id }, obituary);
})
.RequireAuthorization("ApiPolicy")
.WithName("CreateObituary")
.WithTags("Obituaries")
.WithOpenApi();

// PUT /api/obituaries/{id} - Update obituary (requires authentication)
app.MapPut("/api/obituaries/{id:int}", async (int id, ObituaryUpdateDto obituaryDto, ApplicationDbContext context, ClaimsPrincipal user) =>
{
    var obituary = await context.Obituaries.FindAsync(id);
    if (obituary == null)
    {
        return Results.NotFound($"Obituary with ID {id} not found.");
    }

    var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    var isAdmin = user.IsInRole("Admin");
    
    // If CreatedByUserId is null/empty, only Admins may update.
    if ((string.IsNullOrEmpty(obituary.CreatedByUserId) || obituary.CreatedByUserId != userId) && !isAdmin)
    {
        return Results.Problem("You can only update obituaries you created, or you must be an Admin.", statusCode: 403);
    }

    // Validate date logic
    if (obituaryDto.DateOfDeath < obituaryDto.DateOfBirth)
    {
        return Results.BadRequest("Date of Death cannot be before Date of Birth.");
    }

    if (obituaryDto.DateOfBirth > DateTime.Now)
    {
        return Results.BadRequest("Date of Birth cannot be in the future.");
    }

    if (obituaryDto.DateOfDeath > DateTime.Now)
    {
        return Results.BadRequest("Date of Death cannot be in the future.");
    }

    obituary.FullName = obituaryDto.FullName;
    obituary.DateOfBirth = obituaryDto.DateOfBirth;
    obituary.DateOfDeath = obituaryDto.DateOfDeath;
    obituary.Biography = obituaryDto.Biography;
    obituary.Photo = obituaryDto.Photo;
    obituary.UpdatedAt = DateTime.UtcNow;

    try
    {
        await context.SaveChangesAsync();
    }
    catch (DbUpdateConcurrencyException)
    {
        if (!context.Obituaries.Any(e => e.Id == id))
        {
            return Results.NotFound();
        }
        else
        {
            throw;
        }
    }

    return Results.NoContent();
})
.RequireAuthorization("ApiPolicy")
.WithName("UpdateObituary")
.WithTags("Obituaries")
.WithOpenApi();

// DELETE /api/obituaries/{id} - Delete obituary (requires authentication)
app.MapDelete("/api/obituaries/{id:int}", async (int id, ApplicationDbContext context, ClaimsPrincipal user) =>
{

    var obituary = await context.Obituaries.FindAsync(id);
    if (obituary == null)
    {
        return Results.NotFound($"Obituary with ID {id} not found.");
    }

    var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    var isAdmin = user.IsInRole("Admin");
    
    // If CreatedByUserId is null/empty, only Admins may delete.
    if ((string.IsNullOrEmpty(obituary.CreatedByUserId) || obituary.CreatedByUserId != userId) && !isAdmin)
    {
        return Results.Problem("You can only delete obituaries you created, or you must be an Admin.", statusCode: 403);
    }

    context.Obituaries.Remove(obituary);
    await context.SaveChangesAsync();

    return Results.NoContent();
})
.RequireAuthorization("ApiPolicy")
.WithName("DeleteObituary")
.WithTags("Obituaries")
.WithOpenApi();

// GET /api/obituaries/my-obituaries - Get current user's obituaries (requires authentication)
app.MapGet("/api/obituaries/my-obituaries", async (ApplicationDbContext context, ClaimsPrincipal user) =>
{
    // Check if user is authenticated
    if (!user.Identity?.IsAuthenticated ?? true)
    {
        return Results.Unauthorized();
    }

    var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId))
    {
        return Results.Unauthorized();
    }

    var obituaries = await context.Obituaries
        .Where(o => o.CreatedByUserId == userId)
        .ToListAsync();

    return Results.Ok(obituaries);
})
.RequireAuthorization("ApiPolicy")
.WithName("GetMyObituaries")
.WithTags("Obituaries")
.WithOpenApi();

app.Run();
