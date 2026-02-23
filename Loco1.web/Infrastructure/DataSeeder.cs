using System;
using System.Linq;
using System.Threading.Tasks;
using Loco1.Data.Models;                         // EN: ApplicationUser
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Loco1.Web.Infrastructure
    {
    // EN: Seed roles and three users (Owner + Admin + User). Idempotent.
    public static class DataSeeder
        {
        public static async Task SeedAsync(IServiceProvider services)
            {
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DataSeeder");
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var config = services.GetRequiredService<IConfiguration>();

            // EN: allow overrides from appsettings/ENV (Seed:*), fallback to dev defaults
            var ownerEmail = config["Seed:Owner:Email"] ?? "lzl70110@gmail.com";
            var adminEmail = config["Seed:Admin:Email"] ?? "lzl@test.test";
            var userEmail = config["Seed:User:Email"] ?? "test@test.test";
            var defaultPwd = config["Seed:DefaultPassword"] ?? "testtest"; // EN: dev only

            // 1) Ensure roles (idempotent)
            foreach (var roleName in new[] { "Owner", "Admin", "User" })
                await EnsureRoleAsync(roleManager, roleName);

            // 2) Ensure users + role assignment (idempotent)
            var owner = await EnsureUserAsync(userManager, ownerEmail, defaultPwd);
            await EnsureUserInRoleAsync(userManager, owner, "Owner");

            var admin = await EnsureUserAsync(userManager, adminEmail, defaultPwd);
            await EnsureUserInRoleAsync(userManager, admin, "Admin");

            var basic = await EnsureUserAsync(userManager, userEmail, defaultPwd);
            await EnsureUserInRoleAsync(userManager, basic, "User");

            // 3) (Optional) Self-healing: assign 'User' to accounts with no roles
            // EN: helpful if older registrations remained with 0 roles
            var allUsers = userManager.Users.ToList();
            foreach (var u in allUsers)
                {
                var roles = await userManager.GetRolesAsync(u);
                if (roles == null || roles.Count == 0)
                    await userManager.AddToRoleAsync(u, "User");
                }

            logger.LogInformation("Seed completed. Owner={Owner}, Admin={Admin}, User={User}",
                ownerEmail, adminEmail, userEmail);
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
        private static async Task<ApplicationUser> EnsureUserAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string password)
            {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
                {
                user = new ApplicationUser
                    {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,   // EN: dev: skip email confirmation
                    IsDeactivated = false   // EN: ApplicationUser flag (if present)
                    };
                var create = await userManager.CreateAsync(user, password);
                if (!create.Succeeded)
                    throw new InvalidOperationException("Failed to create user " + email + ": " +
                        string.Join("; ", create.Errors.Select(e => $"{e.Code}:{e.Description}")));
                }
            else
                {
                // EN: if the account was soft-deleted previously (email anonymized), keep as-is here;
                // restore is an explicit admin action, not part of seed.
                }

            return user;
            }

        // EN: ensure user is in a role
        private static async Task EnsureUserInRoleAsync(
            UserManager<ApplicationUser> userManager,
            ApplicationUser user,
            string role)
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