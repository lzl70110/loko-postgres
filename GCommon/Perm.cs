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

        // ✅ Locomotives (new module)
        public const string Loco_View = "Locomotives.View";
        public const string Loco_Add = "Locomotives.Add";
        public const string Loco_Edit = "Locomotives.Edit";
        public const string Loco_Delete = "Locomotives.Delete";
        // Note: Added locomotive permissions (view/add/edit/delete)
        }
    }