using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scheduler.Data.Entities
{
	public class JobActivity
	{
		public string Name { get; set; }
		public Guid Id { get; set; }
		public bool Enabled { get; set; }
		public string RunSource { get; set; }
		public string Status { get; set; }
		public Nullable<int> LastExecutedStep { get; set; }
		public Nullable<DateTime> LastStepExecutedDateTime { get; set; }
		public Nullable<TimeSpan> LastStepDuration { get; set; }
		public Nullable<DateTime> QueuedDateTime { get; set; }
		public Nullable<DateTime> StartDateTime { get; set; }
		public Nullable<DateTime> CompletedDateTime { get; set; }
		public Nullable<DateTime> NextRunDateTime { get; set; }
		public string LastRunOutCome { get; set; }
		public string LastOutComeMessage { get; set; }
		public Nullable<DateTime> LastRunDateTime { get; set; }
		public Nullable<TimeSpan> LastRunDuration { get; set; }
	}
}
