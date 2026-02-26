using System.ComponentModel.DataAnnotations;
using Loco1.GCommon.Enums;

namespace Loco1.ViewModels.Locomotives
    {
    public class LocoEditVm
        {
        public int? Id { get; set; }

        [Display(Name = "Locomotive_Number")]
        [Required(ErrorMessage = "Validation_Required")]
        [StringLength(6, ErrorMessage = "Validation_StringLength")]
        public string Number { get; set; } = null!;

        [Display(Name = "Locomotive_Type")]
        [Required(ErrorMessage = "Validation_Required")]
        public LocomotiveType Type { get; set; }

        [Display(Name = "Locomotive_MeasuringUnit")]
        [Required(ErrorMessage = "Validation_Required")]
        public MeasuringUnits MeasuringUnit { get; set; }

        [Display(Name = "Locomotive_FuelCapacity")]
        [Range(0, int.MaxValue, ErrorMessage = "Validation_Range")]
        public int FuelCapacity { get; set; }

        [Display(Name = "Locomotive_AxleCount")]
        [Range(0, 20, ErrorMessage = "Validation_Range")]
        public int AxleCount { get; set; }

        [Display(Name = "Locomotive_TotalEngineHours")]
        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Validation_Range")]
        public decimal TotalEngineHours { get; set; }

        [Display(Name = "Locomotive_TotalWorkingDays")]
        [Range(0, int.MaxValue, ErrorMessage = "Validation_Range")]
        public int TotalWorkingDays { get; set; }

        [Display(Name = "Locomotive_LastPlannedRepairType")]
        [StringLength(128, ErrorMessage = "Validation_StringLength")]
        public string? LastPlannedRepairType { get; set; }

        [Display(Name = "Locomotive_LastPlannedRepairDate")]
        public DateTime? LastPlannedRepairDate { get; set; }

        [Display(Name = "Locomotive_LastAxleMeasurementDate")]
        public DateTime? LastAxleMeasurementDate { get; set; }

        [Display(Name = "Locomotive_InterAxleMeasurementPeriodDays")]
        [Range(0, 366, ErrorMessage = "Validation_Range")]
        public int InterAxleMeasurementPeriodDays { get; set; }
        }
    }