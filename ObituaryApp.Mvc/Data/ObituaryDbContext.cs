using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ObituaryApp.Mvc.Models;

namespace ObituaryApp.Mvc.Data;

public class ObituaryDbContext : IdentityDbContext
{
    public ObituaryDbContext(DbContextOptions<ObituaryDbContext> options)
        : base(options)
    {
    }

    // This line of code maps the Obituaries table to Entity Framework Core
    public DbSet<Obituary> Obituaries => Set<Obituary>();

    //This method for auto-incrementing the primary key
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Obituary>(e =>
        {
            e.Property(o => o.Id).ValueGeneratedOnAdd();
        });
    }
}
