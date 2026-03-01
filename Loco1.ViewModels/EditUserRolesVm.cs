namespace Loco1.ViewModels;

public sealed class EditUserRolesVm
{
    public string UserId { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;
    public bool Owner { get; set; }
    public string OwnerRoleName { get; set; } = "Owner";

    public string Email { get; set; } = string.Empty;
 
    public List<string> AvailableRoles { get; set; } = new();
    public List<string> SelectedRoles { get; set; } = new();


    public List<string> Roles { get; set; } = new();
}