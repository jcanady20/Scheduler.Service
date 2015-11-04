using System;
using System.ComponentModel;

namespace Scheduler.Data
{
	public enum RelativeInterval
	{
		[Description("Not Used")]
		NotUsed = 0,
		[Description("First Day")]
		First = 1,
		[Description("Second Day")]
		Second = 2,
		[Description("Third")]
		Third = 4,
		[Description("Fourth Day")]
		Fourth = 8,
		[Description("Last Day")]
		Last = 16,
	}
}
