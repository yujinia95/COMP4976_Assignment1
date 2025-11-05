using System;
using Microsoft.AspNetCore.Identity;
using ObivtuaryMvcApi.Models;

namespace ObivtuaryMvcApi.Data;

public class IdentitySeedData
{
    public static async Task Initialize(ApplicationDbContext context,
                                        UserManager<IdentityUser> userManager,
                                        RoleManager<IdentityRole> roleManager)
    {
        // Ensure the database is created.
        context.Database.EnsureCreated();

        // 2 roles, admin role and user role
        string adminRole = "Admin";
        string userRole = "User";
        // password for all users
        string password4all = "P@$$w0rd";

        // Check if have admin role, if not create it
        if (await roleManager.FindByNameAsync(adminRole) == null)
        {
            await roleManager.CreateAsync(new IdentityRole(adminRole));
        }

        // Check if have user role, if not create it
        if (await roleManager.FindByNameAsync(userRole) == null)
        {
            await roleManager.CreateAsync(new IdentityRole(userRole));
        }
        // Check if have user "aa@aa.aa", if not create it and add to admin role
        if (await userManager.FindByNameAsync("aa@aa.aa") == null)
        {
            var user = new IdentityUser
            {
                UserName = "aa@aa.aa",
                Email = "aa@aa.aa"
            };

            var result = await userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                await userManager.AddPasswordAsync(user, password4all);
                await userManager.AddToRoleAsync(user, adminRole);
            }
        }

        if (await userManager.FindByNameAsync("uu@uu.uu") == null)
        {
            var user = new IdentityUser
            {
                UserName = "uu@uu.uu",
                Email = "uu@uu.uu"
            };

            var result = await userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                await userManager.AddPasswordAsync(user, password4all);
                await userManager.AddToRoleAsync(user, userRole);
            }
        }

        // Automatically assign "User" role to any existing users who don't have a role
        var allUsers = userManager.Users.ToList();
        foreach (var user in allUsers)
        {
            var roles = await userManager.GetRolesAsync(user);
            if (roles.Count == 0)
            {
                // User has no role, assign "User" role by default
                await userManager.AddToRoleAsync(user, userRole);
            }
        }
        // Seed sample obituaries
        await SeedObituaries(context, userManager);
    }

    private static async Task SeedObituaries(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        // Check if we already have any obituaries
        if (context.Obituaries.Any())
            return;

        // Find a sample user to assign as CreatedByUserId
        var admin = await userManager.FindByEmailAsync("aa@aa.aa");
        var user = await userManager.FindByEmailAsync("uu@uu.uu");
        var adminId = admin?.Id ?? string.Empty;
        var userId = user?.Id ?? string.Empty;

        // Create sample obituary records
        var sampleObituaries = new[]
        {
            new Obituary
            {
                FullName = "John Smith",
                DateOfBirth = new DateTime(1950, 5, 10),
                DateOfDeath = new DateTime(2020, 8, 22),
                Biography = "John was a beloved husband, father, and teacher who dedicated his life to education. His passing leaves a profound void in the hearts of his family, friends, and the countless students whose lives he touched.",
                CreatedByUserId = adminId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Photo = Convert.FromBase64String("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")
            },
            new Obituary
            {
                FullName = "Mary Johnson",
                DateOfBirth = new DateTime(1965, 1, 15),
                DateOfDeath = new DateTime(2023, 4, 5),
                Biography = "Mary loved gardening and volunteering at her local animal shelter.",
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Obituary
            {
                FullName = "David Brown",
                DateOfBirth = new DateTime(1980, 9, 25),
                DateOfDeath = new DateTime(2021, 2, 14),
                Biography = "David was a talented musician who inspired many with his compositions.",
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Obituary
            {
                FullName = "Charlie Kirkland",
                DateOfBirth = new DateTime(1989, 9, 25),
                DateOfDeath = new DateTime(2021, 2, 14),
                Biography = "Charlie was a passionate environmentalist who worked tirelessly to protect natural habitats.",
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Obituary
            {
                FullName = "Max Maxwell",
                DateOfBirth = new DateTime(1995, 12, 1),
                DateOfDeath = new DateTime(2022, 6, 30),
                Biography = "Max was a dedicated engineer who loved solving complex problems.",
                CreatedByUserId = adminId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Obituary
            {
                FullName = "Sophia Williams",
                DateOfBirth = new DateTime(1978, 3, 22),
                DateOfDeath = new DateTime(2020, 11, 18),
                Biography = "Sophia was a talented artist known for her vibrant paintings and sculptures.",
                CreatedByUserId = adminId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Obituary
            {
                FullName = "Marie Johnson",
                DateOfBirth = new DateTime(1960, 7, 30),
                DateOfDeath = new DateTime(2019, 5, 12),
                Biography = "Marie was a passionate chef who delighted many with her culinary creations.",
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        // Add and save
        context.Obituaries.AddRange(sampleObituaries);
        await context.SaveChangesAsync();
    }

}


