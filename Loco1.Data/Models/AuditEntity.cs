// Loco1.Data/Models/AuditEntity.cs
using System.ComponentModel.DataAnnotations;
using static Loco1.GCommon.EntityValidationConstants.AuditEntity;

namespace Loco1.Data.Models
{
    // Base entity with audit and soft-delete metadata
    public abstract class AuditEntity
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreatedOn { get; set; }

        [Required]
        [MaxLength(CreatedByMaxLength)]
        public string CreatedBy { get; set; } = null!;

        public DateTime? ModifiedOn { get; set; }

        [MaxLength(ModifiedByMaxLength)]
        public string? ModifiedBy { get; set; }

        public bool IsDeleted { get; set; } = false;

        [MaxLength(NoteMaxLength)]
        public string? Note { get; set; }

        // Soft-delete audit fields
        public DateTime? DateDeleted { get; set; }      
        [MaxLength(ModifiedByMaxLength)]
        public string? DeletedBy { get; set; }         
    }
}