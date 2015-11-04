using System;
using System.Threading.Tasks;

using Scheduler.Data.Context;
using Scheduler.Data.Entities;

namespace Scheduler.Data.Queries
{
	public static class ActivityQueries
	{
		public static Activity GetActivity(this IContext db, Guid jobId)
		{
			var item = db.Activity.Find(jobId);
			if(item == null)
			{
				item = new Activity() { JobId = jobId };
				db.Activity.Add(item);
				db.SaveChanges();
			}
			return item;
		}

		public static void SetActivityStatus(this IContext db, Guid jobId, JobStatus status)
		{
			var record = db.GetActivity(jobId);
			record.Status = status;
			db.SaveChanges();
		}

		public static void SetActivityLastStepDetails(this IContext db, Guid jobId, int step, DateTime executedDateTime, TimeSpan duration)
		{
			var record = db.GetActivity(jobId);
			record.LastExecutedStep = step;
			record.LastStepExecutedDateTime = executedDateTime;
			record.LastStepDuration = duration;
			db.SaveChanges();
		}

		public static async Task<Activity> GetActivityAsync(this IContext db, Guid jobId)
		{
			var item = await db.Activity.FindAsync(jobId);
			if(item == null)
			{
				item = new Activity() { JobId = jobId };
				db.Activity.Add(item);
				await db.SaveChangesAsync();
			}
			return item;
		}

		public static async Task SetActivityStatusAsync(this IContext db, Guid jobId, JobStatus status)
		{
			var item = await db.GetActivityAsync(jobId);
			item.Status = status;
			await db.SaveChangesAsync();
		}

		public static async Task SetActivityLastStepDetailsAsync(this IContext db, Guid jobId, int step, DateTime executedDateTime, TimeSpan duration)
		{
			var item = await db.GetActivityAsync(jobId);
			item.LastExecutedStep = step;
			item.LastStepExecutedDateTime = executedDateTime;
			item.LastStepDuration = duration;
			await db.SaveChangesAsync();
		}

	}
}
