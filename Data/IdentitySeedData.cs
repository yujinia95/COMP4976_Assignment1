using System;
using Microsoft.AspNetCore.Identity;

namespace ObivtuaryMvcApi.Data;

public class IdentitySeedData {
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
        if (await roleManager.FindByNameAsync(userRole) == null) {
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
            if (result.Succeeded) {
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
    }
}
