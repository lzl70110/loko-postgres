namespace GCommon;

public static class RolesBase
{
    public const string Admin = "Admin";
    public const string DieselLocomotiveRepairManager = "DieselLocomotiveRepairManager";
    public const string DieselLocomotiveRepairSupervisor = "DieselLocomotiveRepairSupervisor";
    public const string LocomotivesDriversManager = "LocomotivesDriversManager";
    public const string LocomotiveTransportManager = "LocomotiveTransportManager";
    public const string Owner = "Owner";
    public const string RailTransportManager = "RailTransportManager";
    public const string User = "User";

    public static readonly string[] All =
    {
        
        Admin,
        RailTransportManager,
        LocomotiveTransportManager,
        DieselLocomotiveRepairManager,
        DieselLocomotiveRepairSupervisor,
        LocomotivesDriversManager,
        User
    };
}