using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Loco1.Web.Infrastructure
    {
    // English: Seed roles and three users (Owner + Admin + User). Idempotent.
    public static class DataSeeder
        {
        public static async Task SeedAsync(IServiceProvider services)
            {
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DataSeeder"); // EN: structured logs
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var config = services.GetRequiredService<IConfiguration>(); // EN: allow secrets/ENV overrides

            // EN: allow overrides from appsettings/ENV (Seed:*), fallback to dev defaults
            var ownerEmail = config["Seed:Owner:Email"] ?? "lzl70110@gmail.com";
            var adminEmail = config["Seed:Admin:Email"] ?? "lzl@test.test";
            var userEmail = config["Seed:User:Email"] ?? "test@test.test";
            var defaultPwd = config["Seed:DefaultPassword"] ?? "testtest"; // EN: dev only

            // 1) Ensure roles
            foreach (var roleName in new[] { "Owner", "Admin", "User" })
                {
                await EnsureRoleAsync(roleManager, roleName); // EN: idempotent role creation
                }

            // 2) Ensure users + role assignment
            var owner = await EnsureUserAsync(userManager, ownerEmail, defaultPwd);
            await EnsureUserInRoleAsync(userManager, owner, "Owner");

            var admin = await EnsureUserAsync(userManager, adminEmail, defaultPwd);
            await EnsureUserInRoleAsync(userManager, admin, "Admin");

            var basic = await EnsureUserAsync(userManager, userEmail, defaultPwd);
            await EnsureUserInRoleAsync(userManager, basic, "User");

            logger.LogInformation("Seed completed. Owner={Owner}, Admin={Admin}, User={User}",
                ownerEmail, adminEmail, userEmail); // EN: final info
            }

        // EN: create role if it doesn't exist
        private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
            {
            if (!await roleManager.RoleExistsAsync(roleName))
                {
                var create = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!create.Succeeded)
                    throw new InvalidOperationException("Failed to create role " + roleName + ": " +
                        string.Join("; ", create.Errors.Select(e => $"{e.Code}:{e.Description}")));
                }
            }

        // EN: create user if not exists (dev-friendly defaults)
        private static async Task<IdentityUser> EnsureUserAsync(UserManager<IdentityUser> userManager, string email, string password)
            {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
                {
                user = new IdentityUser
                    {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true // EN: dev: skip email confirmation
                    };
                var create = await userManager.CreateAsync(user, password);
                if (!create.Succeeded)
                    throw new InvalidOperationException("Failed to create user " + email + ": " +
                        string.Join("; ", create.Errors.Select(e => $"{e.Code}:{e.Description}")));
                }
            return user;
            }

        // EN: ensure user is in a role
        private static async Task EnsureUserInRoleAsync(UserManager<IdentityUser> userManager, IdentityUser user, string role)
            {
            if (!await userManager.IsInRoleAsync(user, role))
                {
                var add = await userManager.AddToRoleAsync(user, role);
                if (!add.Succeeded)
                    throw new InvalidOperationException($"Failed to add {user.Email} to role {role}: " +
                        string.Join("; ", add.Errors.Select(e => $"{e.Code}:{e.Description}")));
                }
            }
        }
    }
