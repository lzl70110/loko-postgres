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
        public int Id { get; set; }

        // FK to Locomotive
        [Required]
        public int LocomotiveId { get; set; }

        [ForeignKey(nameof(LocomotiveId))]
        public Locomotive Locomotive { get; set; } = null!;

        // Local calendar date of the shift (no shift time-range tracking here)
        [Required]
        public DateTime ShiftDate { get; set; }

        // Shift identifier (1st/2nd/3rd) via enum for clarity
        [Required]
        public Shift Shift { get; set; }

        // Engine hours consumed in this shift (backend updates loco counters)
        [Column(TypeName = Dec)]
        public decimal EngineHoursUsed { get; set; }

        // Mainline rule: if true -> +1 working day; Shunter ignores this flag for day calc
        public bool IsAWorkingDay { get; set; }

        }
    }