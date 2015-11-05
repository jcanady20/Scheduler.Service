using System;

namespace Scheduler.Data
{
	public enum JobCompletionAction
	{
		Never = 0,
		OnSuccess,
		OnFailure,
		Always,
	}
}
