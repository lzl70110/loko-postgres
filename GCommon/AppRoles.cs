// GCommon/AppRoles.cs
namespace GCommon
    {
    public static class AppRoles
        {
        public const string Admin = "Admin";
        public const string User = "User";
        public const string Owner = "Owner";
        public const string DieselLocomotiveRepairManager = "DieselLocomotiveRepairManager";
        public const string DieselLocomotiveRepairSupervisor = "DieselLocomotiveRepairSupervisor";
        public const string LocomotivesDriversManager = "LocomotivesDriversManager";
        public const string LocomotiveTransportManager = "LocomotiveTransportManager";
        public const string RailTransportManager = "RailTransportManager";

        // Canonical set (used by seeding, checks, etc.)
        public static readonly string[] All =
        {
            //Admin,
            User,
            Owner,
            DieselLocomotiveRepairManager,
            DieselLocomotiveRepairSupervisor,
            LocomotivesDriversManager,
            LocomotiveTransportManager,
            RailTransportManager
        };

        // Role hierarchy from highest to lowest privilege (used for UI ordering)
        public static readonly string[] Hierarchy =
        {
            
           // Admin,                               // global admin
            RailTransportManager,                // Началник ж.п. транспорт
            LocomotiveTransportManager,          // Началник на Експлоатацията
            DieselLocomotiveRepairManager,       // Нач. Дизелово депо-ремонт
            LocomotivesDriversManager,           // Деломайстор
            DieselLocomotiveRepairSupervisor,    // Ръководител смяна Дизелово депо-ремонт
            User,                                // base user        
            Owner,                               // top business owner
        };
        }
    }
