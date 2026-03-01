namespace Loco1.ViewModels.Roles;

// EN: Used in "Edit user roles" screen (checkboxes per role).
public sealed class RoleListItem
{
    public string Id { get; init; } = string.Empty;  // Role id
    public string? Name { get; init; }               // Role name
    public bool Selected { get; set; }               // Checkbox state

    // Optional aliases if some old code/views still reference RoleId/RoleName:
    public string RoleId => Id;
    public string RoleName => Name ?? string.Empty;
}