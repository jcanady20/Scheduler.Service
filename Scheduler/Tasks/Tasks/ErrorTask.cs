using System;
using System.ComponentModel;

namespace Scheduler.Tasks
{
    [DisplayName("Error Task")]
	[Description("Throws an Exception when Executed. Used to Test error Handling and logging.")]
	public class ErrorTask : JobTaskBase
	{
		public override void OnExecute()
		{
			throw new Exception("Test Error Handler");
		}
	}
}
