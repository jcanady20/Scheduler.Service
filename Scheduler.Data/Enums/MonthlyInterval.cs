using System;
using System.ComponentModel;

namespace Scheduler.Data
{
	[Flags]
	public enum MonthlyInterval
	{
		[Description("Not Used")]
		NotUsed = 0,
		[Description("Sunday")]
		Sunday = 1,
		[Description("Monday")]
		Monday = 2,
		[Description("Tuesday")]
		Tuesday = 4,
		[Description("Wednesday")]
		Wednesday = 8,
		[Description("Thursday")]
		Thursday = 16,
		[Description("Friday")]
		Friday = 32,
		[Description("Saturday")]
		Saturday = 64,
		[Description("Day")]
		Day = Sunday | Monday | Tuesday | Wednesday | Thursday | Friday | Saturday,
		[Description("Week Day")]
		WeekDay = Monday | Tuesday | Wednesday | Thursday | Friday,
		[Description("Weekend Day")]
		WeekendDay = Sunday | Saturday,
	}
}
