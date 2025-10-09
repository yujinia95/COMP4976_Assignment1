using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ObituaryApp.Mvc.Data;
using ObituaryApp.Mvc.Models;
using DotNetEnv;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


//! This is ideal codes for database connection and identity management :)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();


//! Commented out because we don't need this. We have never learned using IdentityCore. Please reference the lecture script from Medhat. Thank you!
// // Add Entity Framework
// builder.Services.AddDbContext<ApplicationDbContext>(options =>
//     options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=app.db"));

// // Add Identity services
// builder.Services.AddIdentityCore<ApplicationUser>(options => 
// {
//     // Password settings
//     options.Password.RequireDigit = true;
//     options.Password.RequireLowercase = true;
//     options.Password.RequireNonAlphanumeric = false;
//     options.Password.RequireUppercase = true;
//     options.Password.RequiredLength = 8;
//     options.Password.RequiredUniqueChars = 1;

//     // Lockout settings
//     options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
//     options.Lockout.MaxFailedAccessAttempts = 5;
//     options.Lockout.AllowedForNewUsers = true;

//     // User settings
//     options.User.AllowedUserNameCharacters =
//         "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
//     options.User.RequireUniqueEmail = true;
// })
// .AddRoles<IdentityRole>() //Enabling role based auth
// .AddEntityFrameworkStores<ApplicationDbContext>()
// .AddSignInManager() // helper functions for managing user sign-ins.
// .AddDefaultTokenProviders();

/*
    JWT Authentication setup
*/
// Read Jwt settings from appsettings.json
var jwt = builder.Configuration.GetSection("Jwt");
// Turns the key from appsettings.json into a cryptographic key.
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!));
// Using JWT Bearer tokens for authentication and unauthenticated requests.
builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(option =>
{   
    // JWT validation rules
    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt["ObituaryApp"],
        ValidAudience = jwt["ObituaryAppUsers"],
        // NO expired tokens!
        IssuerSigningKey = key,
        // Why 30 seconds? Allows 30sec grace period for small time differences between servers
        ClockSkew = TimeSpan.FromSeconds(30)
    };
});


var app = builder.Build();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        DbSeeder.SeedUsersAndRolesAsync(services).GetAwaiter().GetResult();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.Run();
