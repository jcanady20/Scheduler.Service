using System;

namespace Scheduler.Data
{
	public enum JobStatus
	{
        Cacnceled = 0,
		Executing = 1,
		WaitingForWorkerThread,
		BetweenRetries,
		Idle,
		Suspended,
		WaitForStepToFinish,
		PerformingCompletionAction,
	}
}
