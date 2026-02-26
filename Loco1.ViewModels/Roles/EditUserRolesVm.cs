namespace Loco1.ViewModels.Roles;

public class EditUserRolesVm
    {
    public string UserId { get; set; } = default!;
    public string? Email { get; set; } = default!;
    public List<string> AvailableRoles { get; set; } = new();
    public List<string> SelectedRoles { get; set; } = new();
    }