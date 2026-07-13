using Scheduler.Data.Context;

namespace Scheduler.Data.Queries;

public static class JobScheduleQueries
{

  public static void UpdateJobScheduleLastRun(this ScheduleContext db, int jobId, int scheduleId, DateTime lastRunDateTime)
  {
    var js = db.JobSchedules.Find(jobId, scheduleId);
    if (js == null) { return; }
    js.LastRunDateTime = lastRunDateTime;
    db.SaveChanges();
  }
    public static async Task UpdateJobScheduleLastRunAsync(this ScheduleContext db, int jobId, int scheduleId, DateTime lastRunDateTime)
    {
        var js = db.JobSchedules.Find(new { jobId, scheduleId });
        if (js == null) { return; }
        js.LastRunDateTime = lastRunDateTime;
        await db.SaveChangesAsync();
    }
}
