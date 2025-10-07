using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ObituaryApp.Mvc.Models;

namespace ObituaryApp.Mvc.Data;

// This class represents the database context for the Obituary application, including identity management
public class ObituaryDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
{
    public ObituaryDbContext(DbContextOptions<ObituaryDbContext> options)
        : base(options)
    {
    }

    // This line of code maps the Obituaries table to Entity Framework Core
    public DbSet<Obituary> Obituaries => Set<Obituary>();
}
