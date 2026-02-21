namespace Loco1.GCommon;

public static class EntityValidationConstants
    {
    public static class Locomotive
        {
        public const int LocomotiveNumberLength = 6;
        public const string LocomotiveNumberPattern = @"^[0-9]{2}\-[0-9]{3}$";
        
        public const double MinFuelCapacity = 0.1;
        public const double MaxFuelCapacity = 5000.0;
        public const string Dec = "decimal(9,2)";
        public const string DateTimeFormat = "dd-MM-yyyy HH:mm";
      
        public const int AxlesMin = 2;
        public const int AxlesMax = 8;
        }

    public static class AuditEntity

        {
        public const int CreatedByMinLength = 3;
        public const int CreatedByMaxLength = 10;
        public const int ModifiedByMinLength = 3;
        public const int ModifiedByMaxLength = 10;

        public const string Dec = "decimal(9,2)";

        public const int NoteMinLength = 5;  
        public const int NoteMaxLength = 1000;
        }
    public static class ShiftWork
        {
        public const string Dec = "decimal(9,2)";
        public const double ValueMin = 0.0;
        public const double ValueMax = 999999.9;
        public const int NoteMaxLength = 1000;
        public const int NoteMinLength = 5;
        public const int CreatedByMinLength = 3;
        public const int CreatedByMaxLength = 10;
        public const int ModifiedByMinLength = 3;
        public const int ModifiedByMaxLength = 10;
        }

    public static class Fuel
        {
        public const string Dec = "decimal(9,2)";
        public const double ValueMin = 0.0;
        public const double ValueMax = 5000.0;
        public const int NoteMaxLength = 1000;
        public const int NoteMinLength = 5;
        public const int CreatedByMinLength = 3;
        public const int CreatedByMaxLength = 10;
        public const int ModifiedByMinLength = 3;
        public const int ModifiedByMaxLength = 10;
        }


    }