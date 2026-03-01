// Loco1.Web/Infrastructure/PermissionAuthorization.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Loco1.Web.Infrastructure
{
    // Requirement that holds a single permission key (e.g., "Locomotives.Edit")
    public sealed class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }
        public PermissionRequirement(string permission) => Permission = permission;
    }

    // Handler: approves if the user has a matching permission claim
    // (Owner override is handled by OwnerOverrideAuthorizationHandler)
    public sealed class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private static readonly StringComparer Cmp = StringComparer.OrdinalIgnoreCase;

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement req)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
                return Task.CompletedTask;

            // Look for "permission" claims on the user principal
            // (they may come via RoleClaims + ClaimsTransformation or directly attached by Identity)
            foreach (var c in context.User.FindAll("permission"))
            {
                if (Cmp.Equals(c.Value, req.Permission))
                {
                    context.Succeed(req);
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
                    options.AddPolicy(p, policy => policy.Requirements.Add(new PermissionRequirement(p)));
            });

            return services;
        }
    }
}