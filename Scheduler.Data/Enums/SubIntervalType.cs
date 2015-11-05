using System;
using System.ComponentModel;

namespace Scheduler.Data
{
	[Flags]
	public enum SubIntervalType
	{
		[Description("At the Specified Time")]
		SpecificTime = 1,
		[Description("Minutes")]
		Minutes = 4,
		[Description("Hours")]
		Hours = 8
	}
}
