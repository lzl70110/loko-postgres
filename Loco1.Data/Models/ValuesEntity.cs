using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Loco1.GCommon.EntityValidationConstants.Locomotive;

namespace Loco1.Data.Models;
public abstract class ValuesEntity: AuditEntity
    {
    [Column(TypeName = Dec)]
    public Decimal StartValue { get; set; }

    [Column(TypeName = Dec)]
    public Decimal EndValue { get; set; }

    [Column(TypeName = Dec)]
    public decimal Amount { get; set; }
    }
