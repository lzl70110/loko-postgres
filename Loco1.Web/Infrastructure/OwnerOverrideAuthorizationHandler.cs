using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Loco1.Web.Infrastructure
    {
    /// <summary>
    /// Grants all pending authorization requirements when current user is the configured Owner.
    /// </summary>
    public class OwnerOverrideAuthorizationHandler : IAuthorizationHandler
        {
        private readonly string? _ownerEmail;

        public OwnerOverrideAuthorizationHandler(IOptions<OwnerOptions> options)
            {
            _ownerEmail = options.Value.Email;
            }

        public Task HandleAsync(AuthorizationHandlerContext context)
            {
            if (string.IsNullOrWhiteSpace(_ownerEmail))
                return Task.CompletedTask;

            // Typical identity claims: Email or Name
            var email = context.User.FindFirstValue(ClaimTypes.Email)
                        ?? context.User.Identity?.Name;

            if (!string.IsNullOrWhiteSpace(email) &&
                string.Equals(email, _ownerEmail, StringComparison.OrdinalIgnoreCase))
                {
                // Approve all requirements for this evaluation
                foreach (var req in context.PendingRequirements.ToList())
                    {
                    context.Succeed(req);
                    }
                }

            return Task.CompletedTask;
            }
        }
    }