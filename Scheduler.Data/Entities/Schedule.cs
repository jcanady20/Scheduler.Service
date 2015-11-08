using System;

namespace Scheduler.Data.Entities
{
    public class Schedule
    {
        public Schedule()
        {
            Id = Guid.NewGuid();
            Enabled = true;
            Type = Data.FrequencyType.Daily;
            SubdayType = SubIntervalType.Minutes;
            RelativeInterval = RelativeInterval.NotUsed;
            StartDate = new DateTime(1980, 1, 1, 0, 0, 0);
            EndDate = new DateTime(2099, 12, 31, 23, 59, 59);
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public FrequencyType Type { get; set; }
        public int Interval { get; set; }
        public SubIntervalType SubdayType { get; set; }
        public int SubdayInterval { get; set; }
        public RelativeInterval RelativeInterval { get; set; }
        public int RecurrenceFactor { get; set; }
        public DateTime StartDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
