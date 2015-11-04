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

		public static IQueryable<Data.Entities.JobSchedule> GetJobSchedules(this IContext db, Guid jobId)
		{
			return db.JobSchedules.Where(x => x.JobId == jobId);
		}
	}
}
