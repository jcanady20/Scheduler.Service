using System.ComponentModel;
using Scheduler.Tasks;

namespace Scheduler.Scheduling.Tasks
{
    [DisplayName("Sample Task")]
	[Description("Executes a task that Waits 10 seconds before completing")]
	public class EmptyTask : JobTaskBase
	{
		public override void OnExecute()
		{
			//	Sleep for 10 seconds
			System.Threading.Thread.Sleep(10000);
		}
	}
}
