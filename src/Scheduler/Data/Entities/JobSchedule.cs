namespace Scheduler.Data.Entities;

public class JobSchedule
{
    public JobSchedule()
    {
        LastRunDateTime = DateTime.MinValue;
    }
    public int JobId { get; set; }
    public int ScheduleId { get; set; }
    public DateTime LastRunDateTime { get; set; }

    public virtual Job Job { get; set; }
    public virtual Schedule Schedule { get; set; }
}
