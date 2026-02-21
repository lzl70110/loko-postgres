// File: Loco1.Web/ViewModels/UserRoleViewModels.cs
namespace Loco1.ViewModels
    {
    // English: simple projection for the users list
    public class UserWithRolesVm
        {
        public string Id { get; set; } = default!;
        public string Email { get; set; } = default!;
        public List<string> Roles { get; set; } = new();
        }

    
    }