using Microsoft.AspNetCore.Identity;
using ObituaryApp.Mvc.Models;

namespace ObituaryApp.Mvc.Data
{
    public static class DbSeeder
    {
        public static async Task SeedUsersAndRolesAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Seed Roles
            await SeedRolesAsync(roleManager);

            // Seed Users
            await SeedUsersAsync(userManager);
        }
        // Makes sure roles exist before being assigned to users
        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            var roles = new[] { "Admin", "User", "Creator" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
        {
            // Seed Admin User
            var adminEmail = "aa@aa.aa";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    FirstName = "AdminFirstName",
                    LastName = "AdminLastName",
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    Role = "Admin"
                };

                var result = await userManager.CreateAsync(adminUser, "P@$$w0rd");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // seed Creator User
            var creatorEmail = "cc@cc.cc";
            if (await userManager.FindByEmailAsync(creatorEmail) == null)
            {
                var creatorUser = new ApplicationUser
                {
                    FirstName = "CreatorFirstName",
                    LastName = "CreatorLastName",
                    UserName = creatorEmail,
                    Email = creatorEmail,
                    EmailConfirmed = true,
                    Role = "Creator"
                };

                var result = await userManager.CreateAsync(creatorUser, "P@$$w0rd");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(creatorUser, "Creator");
                }
            }

            // Seed Regular User
            var userEmail = "uu@uu.uu";
            if (await userManager.FindByEmailAsync(userEmail) == null)
            {
                var regularUser = new ApplicationUser
                {
                    FirstName = "RegularFirstName",
                    LastName = "RegularLastName",
                    UserName = userEmail,
                    Email = userEmail,
                    EmailConfirmed = true,
                    Role = "User"
                };

                var result = await userManager.CreateAsync(regularUser, "P@$$w0rd");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(regularUser, "User");
                }
            }
        }
    }
}