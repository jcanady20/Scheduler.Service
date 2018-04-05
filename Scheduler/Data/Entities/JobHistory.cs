using System;

namespace Scheduler.Data.Entities
{
	public class JobHistory
	{
		public int Id { get; set; }
		public int JobId { get; set; }
		public Nullable<int> StepId { get; set; }
		public string StepName { get; set; }
		public string Message { get; set; }
		public JobStepOutCome RunStatus { get; set; }
		public Nullable<DateTime> RunDateTime { get; set; }
		public Nullable<TimeSpan> RunDuration { get; set; }
	}
}
