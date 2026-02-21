using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Loco1.GCommon.Enums;
using static Loco1.GCommon.EntityValidationConstants.Locomotive;

namespace Loco1.Data.Models
    {
    // Locomotive entity: only factual, stable properties + navigations
    public class Locomotive : AuditEntity
        {
        [Required]
        [MaxLength(LocomotiveNumberLength)]
        public string Number { get; set; } = null!;

        [Required]
        public LocomotiveType Type { get; set; }

        [Required]
        public MeasuringUnits MeasuringUnit { get; set; }

        public int FuelCapacity { get; set; }

        [Range(AxlesMin, AxlesMax)]
        public int AxleCount { get; set; }

        // Backend-updated counters
        [Column(TypeName = Dec)]
        public decimal TotalEngineHours { get; set; }
        public int TotalWorkingDays { get; set; }

        // Last PLANNED repair (TP1/TP2/MPR)
        public string? LastPlannedRepairType { get; set; }
        public DateTime? LastPlannedRepairDate { get; set; }

        // Axle measurement tracking
        public DateTime? LastAxleMeasurementDate { get; set; }
        public int InterAxleMeasurementPeriodDays { get; set; }

        // Navigations (separate tables for reporting)
        public virtual ICollection<ShiftWork> ShiftWorks { get; set; } = new HashSet<ShiftWork>();
        public virtual ICollection<Fuel> Fuels { get; set; } = new HashSet<Fuel>();

        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new HashSet<AuditLog>();
        }
    }