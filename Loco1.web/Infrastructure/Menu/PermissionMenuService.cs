using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using Loco1.Localizer;

namespace Loco1.Web.Infrastructure.Menu
{
    // Builds a filtered permission menu (implementation to follow)
    public interface IPermissionMenuService
    {
        Task<List<PermissionMenuGroup>> BuildAsync(ClaimsPrincipal user);
    }

    public sealed class PermissionMenuService : IPermissionMenuService
    {
        private readonly IAuthorizationService _auth;
        private readonly IStringLocalizer<SharedResource> _loc;

        public PermissionMenuService(IAuthorizationService auth, IStringLocalizer<SharedResource> loc)
        {
            _auth = auth;
            _loc = loc;
        }

        public Task<List<PermissionMenuGroup>> BuildAsync(ClaimsPrincipal user)
        {
            // TODO: implement in next step
            return Task.FromResult(new List<PermissionMenuGroup>());
        }
    }
}
