using System;
using System.ComponentModel;

namespace Scheduler.Data
{
	[Flags]
	public enum FrequencyType
	{
		[Description("One Time Only")]
		OneTimeOnly = 1,
		[Description("Daily")]
		Daily = 4,
		[Description("Weekly")]
		Weekly = 8,
		[Description("Monthly")]
		Monthly = 16,
		[Description("Monthly, relative to interval")]
		MonthlyRelative = 32,
		[Description("Runs when the Scheduler Agent Starts")]
		OnStartup = 64,
		//[Description("Runs when the computer is Idle")]
		//OnIdle = 128,
	}
}
