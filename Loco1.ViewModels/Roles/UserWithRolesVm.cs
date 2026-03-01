namespace Loco1.ViewModels.Roles
    {
    public class UserWithRolesVm
        {
        public string Id { get; set; } = default!;
        public string Email { get; set; } = default!;
        public List<string> Roles { get; set; } = new();

        // for soft-deletion status; 
        //true if the user is deactivated, false if active
        public bool IsDeactivated { get; set; }
        }
    }