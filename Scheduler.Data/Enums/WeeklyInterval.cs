using System;
using System.ComponentModel;

namespace Scheduler.Data
{
	[Flags]
	public enum WeeklyInterval
	{
		[Description("Not Used")]
		NotUsed = 0,
		Sunday = 1,
		Monday = 2,
		Tuesday = 4,
		Wednesday = 8,
		Thursday = 16,
		Friday = 32,
		Saturday = 64,
	}
}
