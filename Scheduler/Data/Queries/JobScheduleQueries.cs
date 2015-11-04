using System;
using Scheduler.Data.Context;

namespace Scheduler.Data.Queries
{
	public static class JobScheduleQueries
	{

		public static void UpdateJobScheduleLastRun(this IContext db, Guid schuduleId, DateTime lastRunDateTime)
		{
			var js = db.JobSchedules.Find(schuduleId);
			if (js == null) { return; }
			js.LastRunDateTime = lastRunDateTime;
			db.SaveChanges();
		}
	}
}
