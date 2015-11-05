using System;

namespace Scheduler.Data
{
	public enum RunRequestSource
	{
		Scheduler = 1,
		Boot = 3,
		User = 4,
		OnIdle = 6,
	}
}
