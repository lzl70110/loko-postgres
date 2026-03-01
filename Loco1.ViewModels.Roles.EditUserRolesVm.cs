namespace Loco1.ViewModels.Roles
{
    public class EditUserRolesVm
    {
        // Add this property to fix CS1061
        public string RoleId { get; set; }

        // Existing properties
        public string RoleName { get; set; }
        public List<GroupVm> Groups { get; set; }
    }

    public class GroupVm
    {
        public string GroupName { get; set; }
        public List<ItemVm> Items { get; set; }
    }

    public class ItemVm
    {
        public string Code { get; set; }
        public string Display { get; set; }
        public bool Granted { get; set; }
    }
}