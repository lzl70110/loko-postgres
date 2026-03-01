namespace Loco1.ViewModels.Roles;

// EN: Group of permissions for a role edit screen.
public sealed class PermGroupVm
{
    public string GroupName { get; set; } = string.Empty;
    public List<PermItemVm> Items { get; set; } = new();
}