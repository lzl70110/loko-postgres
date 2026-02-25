using GCommon;                         // AppRoles
using Loco1.Data.Models;               // ApplicationUser
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Loco1.Web.Infrastructure
    {
    public static class DataSeeder
        {
        public static async Task SeedAsync(IServiceProvider services)
            {
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DataSeeder");
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var config = services.GetRequiredService<IConfiguration>();

            // 1️⃣ Ensure roles
            foreach (var roleName in AppRoles.All)
                await EnsureRoleAsync(roleManager, roleName);

            // 2️⃣ Seed users from configuration
            var seedUsers = new[]
            {
                new
                {
                    Email = config["Seed:Owner:Email"],
                    FirstName = config["Seed:Owner:FirstName"],
                    LastName = config["Seed:Owner:LastName"],
                    Role = AppRoles.Owner
                },
                new
                {
                    Email = config["Seed:Admin:Email"],
                    FirstName = config["Seed:Admin:FirstName"],
                    LastName = config["Seed:Admin:LastName"],
                    Role = AppRoles.Admin
                },
                new
                {
                    Email = config["Seed:User:Email"],
                    FirstName = config["Seed:User:FirstName"],
                    LastName = config["Seed:User:LastName"],
                    Role = AppRoles.User
                }
            };

            var defaultPwd = config["Seed:DefaultPassword"] ?? "testtest"; // dev only

            foreach (var u in seedUsers)
                {
                if (string.IsNullOrWhiteSpace(u.Email)) continue;

                var user = await EnsureUserAsync(userManager, u.Email, defaultPwd, u.FirstName, u.LastName);
                await EnsureUserInRoleAsync(userManager, user, u.Role);
                }

            // 3️⃣ Self-healing: assign 'User' role to accounts without roles
            var allUsers = await userManager.Users.AsNoTracking().ToListAsync();
            foreach (var u in allUsers)
                {
                var roles = await userManager.GetRolesAsync(u);
                if (roles == null || roles.Count == 0)
                    await userManager.AddToRoleAsync(u, AppRoles.User);
                }

            logger.LogInformation("Seed completed. Roles ensured: {Roles}", string.Join(", ", AppRoles.All));
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
            string password,
            string? firstName = null,
            string? lastName = null)
            {
            var user = await userManager.FindByEmailAsync(email)
                       ?? await userManager.Users.FirstOrDefaultAsync(u => u.OriginalEmail == email);

            if (user is null)
                {
                user = new ApplicationUser
                    {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    EmailConfirmed = true,
                    IsDeactivated = false
                    };

                var create = await userManager.CreateAsync(user, password);
                if (!create.Succeeded)
                    throw new InvalidOperationException("Failed to create user " + email + ": " +
                        string.Join("; ", create.Errors.Select(e => $"{e.Code}:{e.Description}")));
                }
            else
                {
                // Ensure flags are sane
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