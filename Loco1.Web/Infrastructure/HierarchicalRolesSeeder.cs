using Loco1.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Loco1.Web.Infrastructure
    {
    public static class HierarchicalRolesSeeder
        {
        // Синхронизирани роли (Key = internal, DisplayName = за UI)
        public static readonly (string Key, string DisplayName)[] DefaultRoles =
        {
            ("Owner", "Owner"),
            ("Admin", "Administrator"),
            ("RailTransportManager", "Rail Transport Manager"),
            ("LocomotiveTransportManager", "Locomotive Transport Manager"),
            ("DieselLocomotiveRepairManager", "Diesel Locomotives Repair Manager"),
            ("DieselLocomotiveRepairSupervisor", "Diesel Locomotives Repair Supervisor"),
            ("LocomotivesDriversManager", "Locomotives Drivers Manager"),
            ("User", "User")
        };

        public static async Task SeedRolesAsync(IServiceProvider services, IConfiguration? configuration = null)
            {
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("HierarchicalRolesSeeder");
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            // 1️⃣ Load roles from config if provided, else default
            var roles = configuration?.GetSection("Roles:All")?.Get<string[]>()
                        ?? DefaultRoles.Select(r => r.Key).ToArray();

            foreach (var role in roles)
                {
                if (!await roleManager.RoleExistsAsync(role))
                    {
                    var result = await roleManager.CreateAsync(new IdentityRole(role));
                    if (!result.Succeeded)
                        throw new InvalidOperationException($"Failed to create role '{role}': " +
                                                            string.Join("; ", result.Errors.Select(e => $"{e.Code}:{e.Description}")));
                    }
                }

            // 2️⃣ Seed Owner user from config or default
            var ownerEmail = configuration?["Seed:Owner:Email"] ?? "lzl70110@gmail.com";
            var defaultPassword = configuration?["Seed:DefaultPassword"] ?? "testtest";

            var ownerUser = await EnsureUserAsync(userManager, ownerEmail, defaultPassword,
                                                  configuration?["Seed:Owner:FirstName"],
                                                  configuration?["Seed:Owner:LastName"]);

            await EnsureUserInRoleAsync(userManager, ownerUser, "Owner");

            logger.LogInformation("Roles seeded successfully: {Roles}", string.Join(", ", roles));
            }

        private static async Task<ApplicationUser> EnsureUserAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string password,
            string? firstName = null,
            string? lastName = null)
            {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
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

                var createResult = await userManager.CreateAsync(user, password);
                if (!createResult.Succeeded)
                    throw new InvalidOperationException($"Failed to create user '{email}': " +
                                                        string.Join("; ", createResult.Errors.Select(e => $"{e.Code}:{e.Description}")));
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

        private static async Task EnsureUserInRoleAsync(UserManager<ApplicationUser> userManager,
                                                        ApplicationUser user,
                                                        string role)
            {
            if (!await userManager.IsInRoleAsync(user, role))
                {
                var addResult = await userManager.AddToRoleAsync(user, role);
                if (!addResult.Succeeded)
                    throw new InvalidOperationException($"Failed to add user '{user.Email}' to role '{role}': " +
                                                        string.Join("; ", addResult.Errors.Select(e => $"{e.Code}:{e.Description}")));
                }
            }
        }
    }