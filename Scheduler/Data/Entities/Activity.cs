using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scheduler.Data.Entities
{
	public class Activity
	{
		public Guid JobId { get; set; }
		public RunRequestSource RunSource { get; set; }
		public JobStatus Status { get; set; }
		public int LastExecutedStep { get; set; }
		public Nullable<DateTime> LastStepExecutedDateTime { get; set; }
		public Nullable<TimeSpan> LastStepDuration { get; set; }
		public Nullable<DateTime> QueuedDateTime { get; set; }
		public Nullable<DateTime> StartDateTime { get; set; }
		public Nullable<DateTime> CompletedDateTime { get; set; }
		public Nullable<DateTime> NextRunDateTime { get; set; }
		public JobStepOutCome LastRunOutCome { get; set; }
		public string LastOutComeMessage { get; set; }
		public Nullable<DateTime> LastRunDateTime { get; set; }
		public Nullable<TimeSpan> LastRunDuration { get; set; }

		public virtual Job Job { get; set; }
	}
}
