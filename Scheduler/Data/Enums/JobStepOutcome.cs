using System;

namespace Scheduler.Data
{
	public enum JobStepOutCome
	{
		Failed = 0,
		Succeeded = 1,
		Retry = 2,
		Cancelled = 3,
		Unknown = 5,
	}
}
