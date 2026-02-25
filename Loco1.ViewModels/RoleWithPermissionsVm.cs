namespace Loco1.ViewModels
    {
    // ViewModel for displaying a role and its permissions
    public class RoleWithPermissionsVm
        {
        public string Name { get; set; } = default!; // role system name
        public string DisplayName { get; set; } = default!; // visible name in UI
        public List<string> AssignedPermissions { get; set; } = new(); // permissions assigned to this role
        public Dictionary<string, string> AllPermissions { get; set; } = new(); // key=name, value=display
        }
    }