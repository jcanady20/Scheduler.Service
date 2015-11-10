using System;
using Scheduler.Data.Context;
using System.Threading.Tasks;

namespace Scheduler.Data.Queries
{
	public static class JobScheduleQueries
	{

		public static void UpdateJobScheduleLastRun(this IContext db, int jobId, int schuduleId, DateTime lastRunDateTime)
		{
			var js = db.JobSchedules.Find(jobId, schuduleId);
			if (js == null) { return; }
			js.LastRunDateTime = lastRunDateTime;
			db.SaveChanges();
		}
        public static async Task UpdateJobScheduleLastRunAsync(this IContext db, int jobId, int schuduleId, DateTime lastRunDateTime)
        {
            var js = db.JobSchedules.Find(new { jobId, schuduleId });
            if (js == null) { return; }
            js.LastRunDateTime = lastRunDateTime;
            await db.SaveChangesAsync();
        }
    }
}
