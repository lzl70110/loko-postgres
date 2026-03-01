using System.Collections.Generic;

namespace GCommon
{
    // Fine-grained permissions (operation-level)
    public static class Perm
    {
        // Repairs
        public const string Repairs_View = "Repairs.View";
        public const string Repairs_Add = "Repairs.Add";
        public const string Repairs_Edit = "Repairs.Edit";

        // Exploitation (ShiftWork / Driving)
        public const string Expl_View = "Exploitation.View";
        public const string Expl_Add = "Exploitation.Add";
        public const string Expl_Edit = "Exploitation.Edit";

        // Users (admin area)
        public const string Users_View = "Users.View";
        public const string Users_Edit = "Users.Edit";

        // Roles (admin area)
        public const string Roles_View = "Roles.View";
        public const string Roles_Edit = "Roles.Edit";

        // Locomotives (new module)
        public const string Loco_View = "Locomotives.View";
        public const string Loco_Add = "Locomotives.Add";
        public const string Loco_Edit = "Locomotives.Edit";
        public const string Loco_Delete = "Locomotives.Delete";

        // Permission groups for UI mapping
        public static readonly List<PermissionGroup> Groups = new()
        {
            new PermissionGroup
            {
                Name = "Users",
                Items = new List<PermissionItem>
                {
                    new PermissionItem { Code = Users_View, Display = "View Users" },
                    new PermissionItem { Code = Users_Edit, Display = "Edit Users" }
                }
            },
            new PermissionGroup
            {
                Name = "Roles",
                Items = new List<PermissionItem>
                {
                    new PermissionItem { Code = Roles_View, Display = "View Roles" },
                    new PermissionItem { Code = Roles_Edit, Display = "Edit Roles" }
                }
            },
            new PermissionGroup
            {
                Name = "Repairs",
                Items = new List<PermissionItem>
                {
                    new PermissionItem { Code = Repairs_View, Display = "View Repairs" },
                    new PermissionItem { Code = Repairs_Add,  Display = "Add Repairs" },
                    new PermissionItem { Code = Repairs_Edit, Display = "Edit Repairs" }
                }
            },
            new PermissionGroup
            {
                Name = "Exploitation",
                Items = new List<PermissionItem>
                {
                    new PermissionItem { Code = Expl_View, Display = "View Exploitation" },
                    new PermissionItem { Code = Expl_Add,  Display = "Add Exploitation" },
                    new PermissionItem { Code = Expl_Edit, Display = "Edit Exploitation" }
                }
            },
            new PermissionGroup
            {
                Name = "Locomotives",
                Items = new List<PermissionItem>
                {
                    new PermissionItem { Code = Loco_View,   Display = "View Locomotives" },
                    new PermissionItem { Code = Loco_Add,    Display = "Add Locomotives" },
                    new PermissionItem { Code = Loco_Edit,   Display = "Edit Locomotives" },
                    new PermissionItem { Code = Loco_Delete, Display = "Delete Locomotives" }
                }
            }
        };
    }

    public class PermissionGroup
    {
        public string Name { get; set; } = string.Empty;                  // default avoids null warnings
        public List<PermissionItem> Items { get; set; } = new();          // default avoids null warnings
    }

    public class PermissionItem
    {
        public string Code { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
    }
}