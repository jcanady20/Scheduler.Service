using System;
using System.Threading;

using Scheduler.Data;
using Scheduler.Data.Context;
using Scheduler.Data.Entities;

namespace Scheduler.Scheduling
{
    public interface IJobTask : IDisposable
	{
		Guid Id { get; }
		string Name { get; }
		int StepId { get; }
		Nullable<DateTime> Started { get; }
		Nullable<TimeSpan> Duration { get; }
		JobStepOutCome OutCome { get; }
		Nullable<DateTime> Completed { get; }
		IJobTask Create(IContext db, JobStep step);
		JobStepOutCome Execute(CancellationToken cancelToken);
	}
}
