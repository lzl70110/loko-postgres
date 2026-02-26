namespace Loco1.Data.Models
    {
    public class WorkdayCalendar
        {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public bool IsWorkingDay { get; set; }
        public string? Description { get; set; }  
        }
    }