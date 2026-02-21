using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loco1.Data.Models;
public class PlannedRepairSet
    {
    public int Id { get; set; }
    [Required]

    public string PlannedRepairType { get; set; } = null!;

    private readonly string[] ShunterPlannedRepairs = new string[]
        { "Tp1",
    "Tp2",
    "Tp1",
    "Tp2",
    "Tp1",
    "Mpr"
            };
    }
   
