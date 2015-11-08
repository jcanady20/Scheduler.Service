using System;
using System.Threading;

namespace Scheduler.Data.Entities
{
	public class JobStep
	{
		public JobStep()
		{
            IsUserDefined = true;
		}
		public Guid Id { get; set; }
		public Guid JobId { get; set; }
		public bool Enabled { get; set; }
		public int StepId { get; set; }
		public string Name { get; set; }
		public string SubSystem { get; set; }
		public string Command { get; set; }
        public string DataSource { get; set; }
		public string DatabaseName { get; set; }
		public string UserName { get; set; }
		public byte[] Password { get; set; }
		public bool IsUserDefined { get; set; }
        public int RetryAttempts { get; set; }
        public int RetryInterval { get; set; }
	}
}
