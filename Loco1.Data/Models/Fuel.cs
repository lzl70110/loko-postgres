using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Loco1.GCommon.EntityValidationConstants.AuditEntity;

namespace Loco1.Data.Models
    {
    public class Fuel : ValuesEntity
        {
        public int LocomotiveId { get; set; }
        public Locomotive Locomotive { get; set; } = null!;

        [Column(TypeName = Dec)]
        public decimal ReFuel { get; set; }

        [Required]
        public DateTime RecordedOn { get; set; }
        }
    }