namespace Loco1.ViewModels.Roles;

// EN: View model for editing permissions of a single role.
public sealed class RolePermVm
{
    public string RoleId { get; init; } = string.Empty;      // hidden input
    public string RoleName { get; init; } = string.Empty;    // heading
    public List<PermGroupVm> Groups { get; init; } = new();  // grouped permissions
}