using System;
using System.ComponentModel;

namespace Scheduler.Data
{
	public enum NextStepAction
	{
		[Description("Go to the next step")]
		NextStep = 1,
		[Description("Quit the task reporting success")]
		QuitSuccess,
		[Description("Quit the task reporting failure")]
		QuitFailure,
	}
}
