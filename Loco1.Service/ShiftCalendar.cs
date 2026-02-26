using GCommon.Enum;
using Loco1.GCommon.Enum;
using Loco1.Service.Abstractions;

namespace Loco1.Service;

public class ShiftCalendar : IShiftCalendar
    { 
        private readonly ShiftCode[] _mainCycle = new[] { ShiftCode.A, ShiftCode.B, ShiftCode.V, ShiftCode.G };
    private readonly int _cycleLength;

    public DateTime StartDate { get; }
    public ShiftCode StartShift { get; }

    public ShiftCalendar(DateTime startDate, ShiftCode startShift)
        {
        if (!_mainCycle.Contains(startShift))
            throw new ArgumentException("StartShift must be A, B, V or G");

        StartDate = startDate.Date;
        StartShift = startShift;
        _cycleLength = _mainCycle.Length;
        }

    public string GetShiftForDate(ShiftCode code, DateTime date)
        {
        date = date.Date;
        int dayOffset = (int)(date - StartDate).TotalDays;
        int startIndex = Array.IndexOf(_mainCycle, StartShift);
        int dayIndex = (startIndex + dayOffset) % _cycleLength;

        ShiftCode dayShift = _mainCycle[dayIndex];
        ShiftCode nightShift = _mainCycle[(dayIndex - 1 + _cycleLength) % _cycleLength];

        if (code == dayShift) return Shift.Day.ToString();
        if (code == nightShift) return Shift.Night.ToString();
        return " "; // rest
        }

    public Dictionary<DateTime, Dictionary<ShiftCode, string>> GetCalendar(DateTime start, int days)
        {
        var calendar = new Dictionary<DateTime, Dictionary<ShiftCode, string>>();
        for (int i = 0; i < days; i++)
            {
            DateTime date = start.Date.AddDays(i);
            var dayShifts = new Dictionary<ShiftCode, string>();
            foreach (var code in _mainCycle)
                {
                dayShifts[code] = GetShiftForDate(code, date);
                }
            calendar[date] = dayShifts;
            }
        return calendar;
        }
    }
