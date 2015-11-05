using System;
using System.Collections.Generic;
using System.Linq;

namespace Scheduler.Extensions
{
	public static class DateTimeExtensions
	{
		/// <summary>
		/// Converts a System.DateTime object to Unix timestamp
		/// </summary>
		/// <returns>The Unix timestamp</returns>
		public static long ToUnixTimestamp(this DateTime date)
		{
			DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0);
			TimeSpan unixTimeSpan = date - unixEpoch;

			return (long)unixTimeSpan.TotalSeconds;
		}

		public static bool IsWeekday(this DayOfWeek dow)
		{
			switch (dow)
			{
				case DayOfWeek.Sunday:
				case DayOfWeek.Saturday:
					return false;

				default:
					return true;
			}
		}

		public static bool IsWeekend(this DayOfWeek dow)
		{
			return !dow.IsWeekday();
		}

		public static DateTime AddWorkdays(this DateTime startDate, int days)
		{
			// start from a weekday        
			while (startDate.DayOfWeek.IsWeekend())
				startDate = startDate.AddDays(1.0);

			for (int i = 0; i < days; ++i)
			{
				startDate = startDate.AddDays(1.0);

				while (startDate.DayOfWeek.IsWeekend())
					startDate = startDate.AddDays(1.0);
			}
			return startDate;
		}
	}
}
