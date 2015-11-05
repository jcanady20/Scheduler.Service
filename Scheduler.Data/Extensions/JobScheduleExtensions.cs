using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Scheduler.Data;
using Scheduler.Data.Entities;

namespace Scheduler.Data.Extensions
{
	public static class JobScheduleExtensions
	{
		private static readonly DateTime m_maxDateTime = new DateTime(2099, 12, 31, 23, 59, 59);
		private static readonly DateTime m_minDateTime = new DateTime(1980, 1, 1, 0, 0, 0);
        public static DateTime MaxDateTime(this JobSchedule schedule)
        {
            return m_maxDateTime;
        }
        public static DateTime MinDateTime(this JobSchedule schedule)
        {
            return m_minDateTime;
        }
        public static DateTime CalculateNextRun(this JobSchedule schedule, DateTime lastRunDateTime)
		{
            //	If the schedule isn't active
            //	Or the Schedule EndDateTime has already passed
            //	Return DateTime.MinValue
            var endDateTime = schedule.EndDate + schedule.EndTime;
            var startDateTime = schedule.StartDate + schedule.StartTime;
			if (schedule.Enabled == false || endDateTime < DateTime.Now)
			{
				return m_maxDateTime;
			}
			//	If the schedule hasn't started yet
			//	Return the StartDatetime value instead
			if (DateTime.Now < startDateTime)
			{
				return startDateTime;
			}

			DateTime sdt = m_minDateTime;
			switch (schedule.Type)
			{
				case FrequencyType.OneTimeOnly:
					sdt = schedule.CalculateOnTimeOnly(lastRunDateTime);
					break;
				case FrequencyType.Daily:
					sdt = schedule.CalculateDaily(lastRunDateTime);
					break;
				case FrequencyType.Monthly:
					sdt = schedule.CalculateMonthly(lastRunDateTime);
					break;
				case FrequencyType.MonthlyRelative:
					sdt = schedule.CalculateMonthlyRelative(lastRunDateTime);
					break;
				case FrequencyType.Weekly:
					sdt = schedule.CalculateWeekly(lastRunDateTime);
					break;
			}
			return sdt;
		}
		private static DateTime CalculateOnTimeOnly(this JobSchedule scheudle, DateTime lastRunDateTime)
		{
			var startDateTime = scheudle.StartDate + scheudle.StartTime;
			//	If the Last Run DateTime is greater or equal to the Start DateTime the job has already ran
			//	Return the Max Value so the job wont run again.
			if(lastRunDateTime >= startDateTime)
			{
				startDateTime = m_maxDateTime;
			}
			return startDateTime;
		}
		private static DateTime CalculateDaily(this JobSchedule schedule, DateTime lastRunDateTime)
		{
			var dt = schedule.CalculateSubDayInterval(lastRunDateTime);

			// Check to see if we need to roll to the next interval day
			if (dt.TimeOfDay > schedule.EndTime || dt.Day != lastRunDateTime.Day)
			{
				lastRunDateTime = lastRunDateTime.AddDays(schedule.Interval);
				lastRunDateTime = lastRunDateTime.Date + schedule.StartTime;
				return lastRunDateTime;
			}
			else
			{
				if (dt.TimeOfDay < schedule.StartTime)
				{
					dt = dt.Date + schedule.StartTime;
				}
				return dt;
			}
		}
		private static DateTime CalculateWeekly(this JobSchedule schedule, DateTime lastRunDateTime)
		{
			var dt = schedule.CalculateSubDayInterval(lastRunDateTime);

			// Check to see if we need to roll to the next interval day
			if (dt.TimeOfDay > schedule.EndTime || dt.Day != lastRunDateTime.Day)
			{
				lastRunDateTime = lastRunDateTime.Date + schedule.StartTime;

				// Assume we'll reach the end of the week without a match
				bool iswithinWeek = false;
				// Start iterations at the Current Day of the Week
				int i = (int)lastRunDateTime.DayOfWeek;
				//	Loop through the days of the Week
				while (i <= 6)
				{
					//	Get the WeekDay Interval value from the given Day of the Week
					var wd = (WeeklyInterval)Enum.Parse(typeof(WeeklyInterval), dt.DayOfWeek.ToString(), true);
					
					//	use Bit wise & operator to determine of the weekday is within the interval
					if ((schedule.Interval & (int)wd) == 0)
					{
						// No Match was found so Increment the interval to the next day.
						dt = dt.Add(TimeSpan.FromDays(1));
					}
					else
					{
						// Match was found and we're still within this week, so no need to increment Week Interval
						iswithinWeek = true;
						break;
					}
					i++;
				}

				if (schedule.RecurrenceFactor != 0 && iswithinWeek == false)
				{
					// Increment the current date object by the number of weeks (7 days) until the first interval day is reached
					WeeklyInterval fid = GetFirstWeekDayDay(schedule.Interval);
					int wi = schedule.RecurrenceFactor * 7;

					// Increment the Current date object until we reach the end of the week (Sunday)
					// Add Week Interval value
					// Increment the Days until we reach the First selected Day of the week
					int dw = (((int)dt.DayOfWeek - 6) * -1) + wi;
					dt = dt.Add(TimeSpan.FromDays(dw));
					int j = 0;
					while (j < 6)
					{
						if (fid == (WeeklyInterval)Enum.Parse(typeof(WeeklyInterval), dt.DayOfWeek.ToString(), true))
							break;
						else
						{
							dt = dt.Add(TimeSpan.FromDays(1));
						}
						j++;
					}
				}
				lastRunDateTime = dt;
			}
			else // We haven't exceeded the end time range yet so increment the time and continue.
			{
				lastRunDateTime = lastRunDateTime + dt.TimeOfDay;
			}
			return lastRunDateTime;
		}
		private static DateTime CalculateMonthly(this JobSchedule schedule, DateTime lastRunDateTime)
		{
            //  Get the Time when the Job is Supposed to Start
            var timeOfDay = schedule.StartTime;
            var dt = lastRunDateTime;
            //  Add the Recurrence Factory (Number of months between Runs) to the Last Run Date
            //  Add the timeOfDay value to it
            dt = dt.AddMonths(schedule.RecurrenceFactor).Date + timeOfDay;
            //  Create a new DateTime object with the Correct Interval (day of the month) Value
            dt = new DateTime(dt.Year, dt.Month, schedule.Interval, dt.Hour, dt.Minute, dt.Second);
            return dt;
        }
		private static DateTime CalculateMonthlyRelative(this JobSchedule schedule, DateTime lastRunDateTime)
		{
			var intrval = schedule.Interval;
			var endtime = schedule.EndTime;

			var subdayType = schedule.SubdayType;
			var subdayInterval = schedule.SubdayInterval;
			var dt = schedule.CalculateSubDayInterval(lastRunDateTime);


			var rst = dt.TimeOfDay;
			// Check to see if we need to roll to the next interval day
			if (rst > endtime || dt.Day != lastRunDateTime.Day)
			{
				// Set the Next Run Time to the Active Start Time Value
				lastRunDateTime = lastRunDateTime + schedule.StartTime;

				//	How many months should be skipped
				var nmonths = schedule.RecurrenceFactor;

				var i = 0;
				while (i < nmonths)
				{
					//	Calculate the number of days in a given month and increment the current dateTime object
					dt = dt.AddDays(((dt.Day - DateTime.DaysInMonth(dt.Year, dt.Month)) * -1) + 1);
					i++;
				}

				bool resultfound = false;
				//	Create Date Time Array to hold first/second/third/forth day of the week
				List<DateTime> daysofmonth = new List<DateTime>();
				//	Calculate the Monthly Interval
				DateTime _smp = dt;
				int s = 0;
				while (s < DateTime.DaysInMonth(_smp.Year, _smp.Month))
				{
					WeeklyInterval wd = (WeeklyInterval)Enum.Parse(typeof(WeeklyInterval), _smp.DayOfWeek.ToString(), true);
					if ((intrval & (int)wd) != 0) // Non Zero results in a positive match
					{
						daysofmonth.Add(_smp);
						switch ((RelativeInterval)schedule.RelativeInterval)
						{
							case RelativeInterval.First:
								if (daysofmonth.Count == 1)
								{
									resultfound = true;
								}
								break;
							case RelativeInterval.Second:
								if (daysofmonth.Count == 2)
								{
									resultfound = true;
								}
								break;
							case RelativeInterval.Third:
								if (daysofmonth.Count == 3)
								{
									resultfound = true;
								}
								break;
							case RelativeInterval.Fourth:
								if (daysofmonth.Count == 4)
								{
									resultfound = true;
								}
								break;
						}
					}
					if (resultfound == true)
						break;
					_smp.AddDays(1);
					s++;
				}
				// Calculate the Difference between the two days
				TimeSpan _datediff = _smp.Subtract(dt);
				// Add the Difference to the current time object
				dt = dt.Add(_datediff);
				// Set the Value
				lastRunDateTime = dt;
			}
			else // We haven't exceeded the end time range yet so increment the time and continue.
			{
				lastRunDateTime = lastRunDateTime + rst;
			}
			return lastRunDateTime;
		}
		private static DateTime CalculateSubDayInterval(this JobSchedule schedule, DateTime datetime)
		{
			DateTime dt = datetime;
			switch (schedule.SubdayType)
			{
				case SubIntervalType.SpecificTime:
					dt = dt.AddDays(1).Date + schedule.StartTime;
					break;
				case SubIntervalType.Hours:
					dt = dt.AddHours(schedule.SubdayInterval);
					break;
				case SubIntervalType.Minutes:
					dt = dt.AddMinutes(schedule.SubdayInterval);
					break;
			}
			return dt;
		}
		private static WeeklyInterval GetFirstWeekDayDay(int interval)
		{
			var wi = WeeklyInterval.NotUsed;
			foreach (WeeklyInterval wd in Enum.GetValues(typeof(WeeklyInterval)))
			{
				if((interval & (int)wd) != 0)
				{
					wi = wd;
					break;
				}
			}

			return wi;
		}
		public static string Description(this JobSchedule schedule)
		{
			var startDateTime = schedule.StartDate + schedule.StartTime;
			var msg = new StringBuilder();

			msg.Append("Occurs");
			switch (schedule.Type)
			{
				case FrequencyType.Daily:
					msg.Append(schedule.GetSubIntervalDescription());
					break;
				case FrequencyType.Weekly:
					msg.Append(schedule.GetWeeklyDescription());
					break;
				case FrequencyType.Monthly:
					msg.Append(schedule.GetMonthlyDescription());
					break;
				case FrequencyType.MonthlyRelative:
					msg.Append(schedule.GetMonthlyRelativeDescription());
					break;
			}
			msg.AppendFormat(" Schedule will be used starting on {0}", startDateTime.ToShortDateString());
			return msg.ToString();
		}
		private static StringBuilder GetSubIntervalDescription(this JobSchedule schedule)
		{
			var msg = new StringBuilder();
			var startDateTime = schedule.StartDate + schedule.StartTime;
			var endDatetime = schedule.EndDate + schedule.EndTime;
			switch (schedule.SubdayType)
			{
				case SubIntervalType.SpecificTime:
					msg.AppendFormat(" at {0}", startDateTime.ToShortTimeString());
					break;
				case SubIntervalType.Minutes:
				case SubIntervalType.Hours:
					msg.AppendFormat(" day every {0} {1}(s) between {2} and {3}.", schedule.SubdayInterval, schedule.SubdayType, startDateTime.ToShortTimeString(), endDatetime.ToShortTimeString());
					break;
			}

			return msg;
		}
		private static StringBuilder GetWeeklyDescription(this JobSchedule schedule)
		{
			var msg = new StringBuilder();
			msg.AppendFormat(" {0} week(s)", schedule.Interval);
			msg.Append(" on ");
			msg.Append(String.Join(",", Enum.GetValues(typeof(WeeklyInterval)).OfType<WeeklyInterval>().Where(x => ((int)x & schedule.Interval) != 0)));
			msg.Append(schedule.GetSubIntervalDescription());
			return msg;
		}
		private static StringBuilder GetMonthlyDescription(this JobSchedule schedule)
		{
			var msg = new StringBuilder();
			msg.AppendFormat(" {0} month(s) on day {1} of that month", schedule.RecurrenceFactor, schedule.Interval);
			msg.Append(schedule.GetSubIntervalDescription());
			return msg;
		}
		private static StringBuilder GetMonthlyRelativeDescription(this JobSchedule schedule)
		{
			var msg = new StringBuilder();
			msg.AppendFormat(" {0} {1} of every {2} month(s)", schedule.RelativeInterval, (MonthlyInterval)schedule.Interval, schedule.RecurrenceFactor);
			msg.Append(schedule.GetSubIntervalDescription());
			return msg;
		}
	}
}
