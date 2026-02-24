using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Loco1.GCommon.Enums; // Shift enum
 
using static Loco1.GCommon.EntityValidationConstants.AuditEntity;  // NoteMaxLength

namespace Loco1.Data.Models
    {
    // Work/shift record for a locomotive (no fuel here; fuel is separate for reporting)
    public class ShiftWork : AuditEntity
        {
        public int LocomotiveId { get; set; }
        [Required]
        [ForeignKey(nameof(LocomotiveId))]
        public Locomotive Locomotive { get; set; } = null!;
        public DateTime ShiftDate { get; set; }
        public Shift Shift { get; set; } = Shift.Day;   // Day | Night
        public decimal EngineHoursUsed { get; set; }
        public bool IsAWorkingDay { get; set; }
        }
    }