using System;
using System.Linq;
using System.Data.Entity;
using Scheduler.Data.Context;

namespace Scheduler.Data.Queries
{
	public static class JobQueries
	{
		public static IQueryable<Data.Entities.Job> GetTJobs(this IContext db)
		{
			return db.Jobs.Include(x => x.JobSchedules).Where(x => x.Enabled == true);
		}

		public static IQueryable<Data.Entities.Schedule> GetJobSchedules(this IContext db, int jobId)
		{
			return db.JobSchedules.Include(x => x.Schedule).Where(x => x.JobId == jobId).Select(r => r.Schedule);
		}
    }
}
