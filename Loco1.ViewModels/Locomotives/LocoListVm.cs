using System.ComponentModel.DataAnnotations;
using Loco1.GCommon.Enums;

namespace Loco1.ViewModels.Locomotives
    {
    public class LocoListVm
        {
        public int Id { get; set; }

        [Display(Name = "Locomotive_Number")]
        public string Number { get; set; } = null!;

        [Display(Name = "Locomotive_Type")]
        public LocomotiveType Type { get; set; }

        [Display(Name = "Locomotive_MeasuringUnit")]
        public MeasuringUnits MeasuringUnit { get; set; }

        [Display(Name = "Locomotive_AxleCount")]
        public int AxleCount { get; set; }

        [Display(Name = "Locomotive_TotalEngineHours")]
        public decimal TotalEngineHours { get; set; }
        }
    }