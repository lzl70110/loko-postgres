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

        // Exploitation
        public const string Expl_View = "Exploitation.View";
        public const string Expl_Add = "Exploitation.Add";
        public const string Expl_Edit = "Exploitation.Edit";

        // Users (admin area)
        public const string Users_View = "Users.View";
        public const string Users_Edit = "Users.Edit";

        // Roles (admin area)
        public const string Roles_View = "Roles.View";
        public const string Roles_Edit = "Roles.Edit";

        // Locomotives
        public const string Loco_View = "Locomotives.View";
        public const string Loco_Add = "Locomotives.Add";
        public const string Loco_Edit = "Locomotives.Edit";
        public const string Loco_Delete = "Locomotives.Delete";

        // UI groups with resource keys (no hard-coded text)
        public static readonly List<PermissionGroup> Groups = new()
        {
            new PermissionGroup
            {
                NameKey = "Group_Users",
                Items = new List<PermissionItem>
                {
                    new PermissionItem { Code = Users_View, ResourceKey = "Perm_Users_View" },
                    new PermissionItem { Code = Users_Edit, ResourceKey = "Perm_Users_Edit" }
                }
            },
            new PermissionGroup
            {
                NameKey = "Group_Roles",
                Items = new List<PermissionItem>
                {
                    new PermissionItem { Code = Roles_View, ResourceKey = "Perm_Roles_View" },
                    new PermissionItem { Code = Roles_Edit, ResourceKey = "Perm_Roles_Edit" }
                }
            },
            new PermissionGroup
            {
                NameKey = "Group_Repairs",
                Items = new List<PermissionItem>
                {
                    new PermissionItem { Code = Repairs_View, ResourceKey = "Perm_Repairs_View" },
                    new PermissionItem { Code = Repairs_Add,  ResourceKey = "Perm_Repairs_Add"  },
                    new PermissionItem { Code = Repairs_Edit, ResourceKey = "Perm_Repairs_Edit" }
                }
            },
            new PermissionGroup
            {
                NameKey = "Group_Exploitation",
                Items = new List<PermissionItem>
                {
                    new PermissionItem { Code = Expl_View, ResourceKey = "Perm_Expl_View" },
                    new PermissionItem { Code = Expl_Add,  ResourceKey = "Perm_Expl_Add"  },
                    new PermissionItem { Code = Expl_Edit, ResourceKey = "Perm_Expl_Edit" }
                }
            },
            new PermissionGroup
            {
                NameKey = "Group_Locomotives",
                Items = new List<PermissionItem>
                {
                    new PermissionItem { Code = Loco_View,   ResourceKey = "Perm_Loco_View"   },
                    new PermissionItem { Code = Loco_Add,    ResourceKey = "Perm_Loco_Add"    },
                    new PermissionItem { Code = Loco_Edit,   ResourceKey = "Perm_Loco_Edit"   },
                    new PermissionItem { Code = Loco_Delete, ResourceKey = "Perm_Loco_Delete" }
                }
            }
        };
    }

    public class PermissionGroup
    {
        public string NameKey { get; set; } = string.Empty;       // resource key for group header
        public List<PermissionItem> Items { get; set; } = new();
    }

    public class PermissionItem
    {
        public string Code { get; set; } = string.Empty;          // permission code (claim value)
        public string ResourceKey { get; set; } = string.Empty;   // resource key for display text
    }
}