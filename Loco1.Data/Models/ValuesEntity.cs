using System.ComponentModel.DataAnnotations.Schema;
using static Loco1.GCommon.EntityValidationConstants.Locomotive;

namespace Loco1.Data.Models;

public abstract class ValuesEntity : AuditEntity
    {
    [Column(TypeName = Dec)]
    public decimal StartValue { get; set; }

    [Column(TypeName = Dec)]
    public decimal EndValue { get; set; }

    [Column(TypeName = Dec)]
    public decimal Amount { get; set; }
    }