using Microsoft.EntityFrameworkCore;
using Scheduler.Data.Context;

namespace Scheduler.Data.Queries;

public static class JobQueries
{
  public static IQueryable<Data.Entities.Job> GetTJobs(this ScheduleContext db)
  {
    return db.Jobs.Include(x => x.JobSchedules).Where(x => x.Enabled == true);
  }

  public static IQueryable<Data.Entities.Schedule> GetJobSchedules(this ScheduleContext db, int jobId)
  {
    return db.JobSchedules.Include(x => x.Schedule).Where(x => x.JobId == jobId).Select(r => r.Schedule);
  }
}
