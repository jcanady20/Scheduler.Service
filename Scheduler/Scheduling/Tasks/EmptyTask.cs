using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Scheduling.Tasks
{
	[DisplayName("Sample Task")]
	[Description("Executes a task that Waits 10 seconds before completing")]
	public class EmptyTask : BaseTask
	{
		public override void OnExecute()
		{
			//	Sleep for 10 seconds
			System.Threading.Thread.Sleep(10000);
		}
	}
}
