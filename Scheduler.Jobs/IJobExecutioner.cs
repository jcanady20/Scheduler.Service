using System;
using Scheduler.Data;

namespace Scheduler.Jobs
{
    public interface IJobExecutioner : IDisposable
	{
		Guid JobId { get; }
		string Name { get; }
		JobStepOutCome OutCome { get; }
		Nullable<DateTime> StartDateTime { get; }
		Nullable<DateTime> CompletedDateTime { get; }
		Nullable<TimeSpan> Duration { get; }
		JobStatus Status { get; }
		void Cancel();
		void Execute();
	}
}
