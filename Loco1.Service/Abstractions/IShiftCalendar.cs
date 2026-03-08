using GCommon.Enum;
using Loco1.GCommon.Enum;



namespace Loco1.Service.Abstractions;
public interface IShiftCalendar
    {
    /// <summary>
    /// Get the shift for a specific shift code on a given date.
    /// Returns Shift.Day, Shift.Night, or empty string " " for rest.
    /// </summary>
    string GetShiftForDate(ShiftCode code, DateTime date);

    /// <summary>
    /// Get full shift calendar for a given range.
    /// </summary>
    Dictionary<DateTime, Dictionary<ShiftCode, string>> GetCalendar(DateTime start, int days);
    }


    