using System;

namespace Scheduler.Data
{
	public enum JobStatus
	{
		Executing = 1,
		WaitingForWorkerThread,
		BetweenRetries,
		Idle,
		Suspended,
		WaitForStepToFinish,
		PerformingCompletionAction,
	}
}
