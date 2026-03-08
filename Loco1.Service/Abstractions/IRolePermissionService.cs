using Loco1.ViewModels.Roles;

public interface IRolePermissionService
{
    // Builds RolePermVm (role-level view: list of groups)
    RolePermVm BuildRolePermVm(string roleId);

    // Builds a single group's VM for editing
    PermGroupVm BuildGroupVm(string roleId, string groupName);

    // Persists changes for the group permissions
    void UpdateGroup(string roleId, string groupName, List<PermItemVm> items);
}