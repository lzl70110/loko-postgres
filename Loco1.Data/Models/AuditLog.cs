using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Loco1.GCommon.EntityValidationConstants.AuditEntity;

namespace Loco1.Data.Models
    {
    [Table("AuditLogs")]
    public class AuditLog
        {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(CreatedByMaxLength)]
        public string User { get; set; } = null!;

        [Required]
        [MaxLength(32)]
        public string Action { get; set; } = null!;

        [Required]
        [MaxLength(128)]
        public string EntityName { get; set; } = null!;

        public int EntityId { get; set; }

        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public virtual Locomotive? Locomotive { get; set; }
        public virtual Fuel? Fuel { get; set; }
        public virtual ShiftWork? ShiftWork { get; set; }
        }
    }