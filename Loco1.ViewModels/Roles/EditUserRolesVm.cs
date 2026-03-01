namespace Loco1.ViewModels.Roles;

public sealed class EditUserRolesVm
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public bool Owner { get; set; }

    // EN: Role name used as the "owner" key in UI/JS
    public string OwnerRoleName { get; set; } = "Owner";

    public List<string> AvailableRoles { get; set; } = new();
    public List<string> SelectedRoles { get; set; } = new();

    public List<RoleListItem> Roles { get; set; } = new();
}
