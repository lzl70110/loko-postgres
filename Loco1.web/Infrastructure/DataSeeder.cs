using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Loco1.Web.Infrastructure
    {
    // English: Seed roles and two users (Admin + User). Idempotent.
    public static class DataSeeder
        {
        public static async Task SeedAsync(IServiceProvider services)
            {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

            const string adminRole = "Admin";
            const string userRole = "User";

            // 1) Ensure roles
            if (!await roleManager.RoleExistsAsync(adminRole))
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            if (!await roleManager.RoleExistsAsync(userRole))
                await roleManager.CreateAsync(new IdentityRole(userRole));

            // Common weak dev password per your request
            const string devPassword = "testtest";

            // 2) Ensure Admin
            const string adminEmail = "lzl@test.test";
            const string adminUserName = "lzl@test.test";

            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin is null)
                {
                admin = new IdentityUser
                    {
                    UserName = adminUserName,
                    Email = adminEmail,
                    EmailConfirmed = true // dev: skip email confirmation
                    };

                var createAdmin = await userManager.CreateAsync(admin, devPassword);
                if (!createAdmin.Succeeded)
                    {
                    var errors = string.Join("; ", createAdmin.Errors.Select(e => $"{e.Code}:{e.Description}"));
                    throw new InvalidOperationException($"Failed to create admin user: {errors}");
                    }
                }

            if (!await userManager.IsInRoleAsync(admin, adminRole))
                await userManager.AddToRoleAsync(admin, adminRole);

            // 3) Ensure basic User
            const string userEmail = "test@test.test";
            const string userUserName = "test@test.test";

            var basic = await userManager.FindByEmailAsync(userEmail);
            if (basic is null)
                {
                basic = new IdentityUser
                    {
                    UserName = userUserName,
                    Email = userEmail,
                    EmailConfirmed = true // dev: skip email confirmation
                    };

                var createUser = await userManager.CreateAsync(basic, devPassword);
                if (!createUser.Succeeded)
                    {
                    var errors = string.Join("; ", createUser.Errors.Select(e => $"{e.Code}:{e.Description}"));
                    throw new InvalidOperationException($"Failed to create basic user: {errors}");
                    }
                }

            if (!await userManager.IsInRoleAsync(basic, userRole))
                await userManager.AddToRoleAsync(basic, userRole);
            }
        }
    }