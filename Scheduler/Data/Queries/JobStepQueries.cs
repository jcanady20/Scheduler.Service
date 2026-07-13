using Scheduler.Data.Context;

namespace Scheduler.Data.Queries;

public static class JobStepQueries
{
  public static IQueryable<Data.Entities.JobStep> GetJobSteps(this ScheduleContext db, int jobId)
  {
    return db.JobSteps.Where(x => x.JobId == jobId);
  }
}
