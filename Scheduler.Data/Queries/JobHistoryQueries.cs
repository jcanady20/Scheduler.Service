using System;
using System.Linq;

using Scheduler.Data.Context;
using Scheduler.Data.Entities;
using System.Threading.Tasks;

namespace Scheduler.Data.Queries
{
	public static class JobHistoryQueries
	{
		public static PagedList<JobHistory> JobHistorySearch(this IContext db, Models.JobHistorySearch model)
		{
			var pageSize = (model.PageSize == 0 ? 10 : model.PageSize);
			var skip = pageSize * (model.Page - 1);
			var take = pageSize;

			var qry = db.JobHistory.Where(x => x.JobId == model.JobId);

			var totalCount = qry.Count();

			if (String.IsNullOrEmpty(model.Term) == false)
			{
				qry = qry.Where(x => x.Message.Contains(model.Term) || x.StepName.StartsWith(model.Term));
			}
			var filterCount = qry.Count();
			var items = qry.OrderByDescending(x => x.RunDateTime).Skip(skip).Take(take).ToList();

			return new PagedList<JobHistory>
			{
				Entities = items,
				TotalCount = totalCount,
				TotalPages = (int)Math.Ceiling(((double)filterCount / (double)take))
			};

		}

		public static void AddJobHistory(this IContext db, Guid jobId, int stepId, string stepName, JobStepOutCome outcome, string message, Nullable<DateTime> runDateTime = null, Nullable<TimeSpan> duration = null)
		{
			var jh = new JobHistory()
			{
				JobId = jobId,
				StepId = stepId,
				StepName = stepName,
				RunStatus = outcome,
				Message = message,
				RunDuration = duration,
				RunDateTime = runDateTime,
			};
			db.JobHistory.Add(jh);
			db.SaveChanges();
		}

        public static async Task AddJobHistoryAsync(this IContext db, Guid jobId, int stepId, string stepName, JobStepOutCome outcome, string message, Nullable<DateTime> runDateTime = null, Nullable<TimeSpan> duration = null)
        {
            var jh = new JobHistory()
            {
                JobId = jobId,
                StepId = stepId,
                StepName = stepName,
                RunStatus = outcome,
                Message = message,
                RunDuration = duration,
                RunDateTime = runDateTime,
            };
            db.JobHistory.Add(jh);
            await db.SaveChangesAsync();
        }
    }
}
