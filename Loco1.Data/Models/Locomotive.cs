using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Loco1.GCommon.Enums;
using static Loco1.GCommon.EntityValidationConstants.Locomotive;

namespace Loco1.Data.Models
    {
    public class Locomotive : AuditEntity
        {
        [MaxLength(LocomotiveNumberLength)]
        public string Number { get; set; } = null!;

        public LocomotiveType Type { get; set; }

        public MeasuringUnits MeasuringUnit { get; set; }

        public int FuelCapacity { get; set; }

        [Range(AxlesMin, AxlesMax)]
        public int AxleCount { get; set; }

        [Column(TypeName = Dec)]
        public decimal TotalEngineHours { get; set; }

        public int TotalWorkingDays { get; set; }

        public string? LastPlannedRepairType { get; set; }
        public DateTime? LastPlannedRepairDate { get; set; }

        public DateTime? LastAxleMeasurementDate { get; set; }
        public int InterAxleMeasurementPeriodDays { get; set; }

        public virtual ICollection<ShiftWork> ShiftWorks { get; set; } = new HashSet<ShiftWork>();
        public virtual ICollection<Fuel> Fuels { get; set; } = new HashSet<Fuel>();

        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new HashSet<AuditLog>();
        }
    }