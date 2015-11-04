using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Scheduling.Tasks
{
	[DisplayName("Error Task")]
	[Description("Throws an Exception when Executed. Used to Test error Handling and logging.")]
	public class ErrorTask : BaseTask
	{
		public override void OnExecute()
		{
			throw new Exception("Test Error Handler");
		}
	}
}
