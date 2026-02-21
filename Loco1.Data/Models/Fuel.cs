using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loco1.Data.Models;
using static Loco1.GCommon.EntityValidationConstants.AuditEntity;
public class Fuel :  ValuesEntity

    {
 
    public int LocomotiveId { get; set; }
    public Locomotive Locomotive { get; set; } = null!;
    [Column(TypeName =Dec)]
    public decimal ReFuel { get; set; }

    [Required]
    public DateTime RecordedOn { get; set; }


    }
