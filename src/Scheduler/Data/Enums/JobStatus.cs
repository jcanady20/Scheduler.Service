namespace Scheduler.Data;

public enum JobStatus
{
  Canceled = 0,
  Executing = 1,
  WaitingForWorkerThread,
  BetweenRetries,
  Idle,
  Suspended,
  WaitForStepToFinish,
  PerformingCompletionAction,
}
