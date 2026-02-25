// Loco1.Web/Infrastructure/PermissionAuthorization.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Loco1.Web.Infrastructure
    {
    // Requirement that holds a single permission key (e.g., "Locomotives.Add")
    public sealed class PermissionRequirement : IAuthorizationRequirement
        {
        public string Permission { get; }
        public PermissionRequirement(string permission) => Permission = permission;
        }

    // Handler: approves if any of user's roles grant the permission (from config)
    public sealed class PermissionHandler : AuthorizationHandler<PermissionRequirement>
        {
        private readonly IConfiguration _cfg;
        public PermissionHandler(IConfiguration cfg) => _cfg = cfg;

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement req)
            {
            if (context.User?.Identity?.IsAuthenticated != true)
                return Task.CompletedTask;

            var map = _cfg.GetSection("Permissions");
            foreach (var roleClaim in context.User.FindAll(ClaimTypes.Role))
                {
                var role = roleClaim.Value;
                var rolePerms = map.GetSection(role).Get<string[]>();
                if (rolePerms != null && rolePerms.Contains(req.Permission, StringComparer.Ordinal))
                    {
                    context.Succeed(new PermissionRequirement(req.Permission));
                    break;
                    }
                }
            return Task.CompletedTask;
            }
        }

    public static class PermissionAuthExtensions
        {
        /// <summary>
        /// Registers permission policies and the PermissionHandler.
        /// Policy name equals the permission key (e.g., "Locomotives.Edit").
        /// </summary>
        public static IServiceCollection AddPermissionPolicies(this IServiceCollection services, params string[] permissions)
            {
            services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

            services.AddAuthorization(options =>
            {
                foreach (var p in permissions.Distinct(StringComparer.Ordinal))
                    {
                    options.AddPolicy(p, policy => policy.Requirements.Add(new PermissionRequirement(p)));
                    }
            });

            return services;
            }
        }
    }