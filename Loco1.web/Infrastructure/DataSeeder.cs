using Loco1.Data.Models;                  // ApplicationUser
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;      // for FirstOrDefaultAsync / AsNoTracking
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Loco1.Web.Infrastructure
    {
    public static class DataSeeder
        {
        private static class Roles
            {
            public const string Owner = "Owner";
            public const string Admin = "Admin";
            public const string User = "User";
            public static readonly string[] All = { Owner, Admin, User };
            }

        public static async Task SeedAsync(IServiceProvider services)
            {
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DataSeeder");
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var config = services.GetRequiredService<IConfiguration>();

            // read seed settings (dev-safe defaults)
            var ownerEmail = config["Seed:Owner:Email"] ?? "lzl70110@gmail.com";
            var adminEmail = config["Seed:Admin:Email"] ?? "lzl@test.test";
            var userEmail = config["Seed:User:Email"] ?? "test@test.test";
            var defaultPwd = config["Seed:DefaultPassword"] ?? "testtest"; // dev only

            // 1) ensure roles
            foreach (var roleName in Roles.All)
                await EnsureRoleAsync(roleManager, roleName);

            // 2) ensure users and role membership (idempotent)
            var owner = await EnsureUserAsync(userManager, ownerEmail, defaultPwd);
            await EnsureUserInRoleAsync(userManager, owner, Roles.Owner);

            var admin = await EnsureUserAsync(userManager, adminEmail, defaultPwd);
            await EnsureUserInRoleAsync(userManager, admin, Roles.Admin);

            var basic = await EnsureUserAsync(userManager, userEmail, defaultPwd);
            await EnsureUserInRoleAsync(userManager, basic, Roles.User);

            // 3) self-healing: assign 'User' to accounts without roles
            //    (helps for older registrations with 0 roles)
            var allUsers = await userManager.Users.AsNoTracking().ToListAsync();
            foreach (var u in allUsers)
                {
                var roles = await userManager.GetRolesAsync(u);
                if (roles == null || roles.Count == 0)
                    {
                    await userManager.AddToRoleAsync(u, Roles.User);
                    }
                }

            logger.LogInformation("Seed completed. Owner={Owner}, Admin={Admin}, User={User}",
                ownerEmail, adminEmail, userEmail);
            }

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

        private static async Task<ApplicationUser> EnsureUserAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string password)
            {
            // try exact email; if anonymized previously, try by OriginalEmail
            var user = await userManager.FindByEmailAsync(email)
                       ?? await userManager.Users.FirstOrDefaultAsync(u => u.OriginalEmail == email);

            if (user is null)
                {
                user = new ApplicationUser
                    {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,   // dev: skip email confirmation
                    IsDeactivated = false
                    };

                var create = await userManager.CreateAsync(user, password);
                if (!create.Succeeded)
                    throw new InvalidOperationException("Failed to create user " + email + ": " +
                        string.Join("; ", create.Errors.Select(e => $"{e.Code}:{e.Description}")));
                }
            else
                {
                // ensure basic flags are sane after restore/older states
                if (user.IsDeactivated)
                    {
                    await userManager.SetLockoutEnabledAsync(user, false);
                    await userManager.SetLockoutEndDateAsync(user, null);
                    user.IsDeactivated = false;
                    await userManager.UpdateAsync(user);
                    }
                }

            return user;
            }

        private static async Task EnsureUserInRoleAsync(
            UserManager<ApplicationUser> userManager,
            ApplicationUser user,
            string role)
            {
            // skip owner assignment downgrades here; this is only additive
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