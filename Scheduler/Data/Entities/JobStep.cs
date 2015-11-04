using System;
using System.Threading;

namespace Scheduler.Data.Entities
{
	public class JobStep
	{
		public JobStep()
		{
			IsVisShipped = false;
		}
		public Guid Id { get; set; }
		public Guid JobId { get; set; }
		public bool Enabled { get; set; }
		public int StepId { get; set; }
		public string Name { get; set; }
		public string SubSystem { get; set; }
		public string Command { get; set; }
		public string DatabaseName { get; set; }
		public string UserName { get; set; }
		public byte[] Password { get; set; }
		public bool IsVisShipped { get; set; }
	}
}
