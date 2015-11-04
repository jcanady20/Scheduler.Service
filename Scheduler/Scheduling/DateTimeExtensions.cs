using System;
using Visualutions.Scheduler.Data;


namespace Visualutions.Scheduler.Scheduling
{
	public static class DateTimeExtensions
	{
		public static int DateToInt(this DateTime datetime)
		{
			int r = 0;
			r = (datetime.Year * 10000) + (datetime.Month * 100) + datetime.Day;
			return r;
		}

		public static int TimeToInt(this DateTime datetime)
		{
			int r = 0;
			r = (datetime.Hour * 10000) + (datetime.Minute * 100) + datetime.Second;
			return r;
		}

		public static DateTime FromIntToDate(this DateTime datetime, int date)
		{
			if (date < 10000000)
				throw new ArgumentOutOfRangeException("Invalid Date");

			int yyyy = date / 10000;
			if (yyyy > DateTime.MaxValue.Year)
				throw new ArgumentOutOfRangeException("Year");

			int mm = date % 10000 / 100;
			int dd = date % 10000 % 100;
			return new DateTime(yyyy, mm, dd);
		}

		public static DateTime FromIntToTime(this DateTime datetime, int time)
		{
			int hh = time / 10000;
			int mi = time % 10000 / 100;
			int ss = time % 10000 % 100;
			return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hh, mi, ss);
		}

		public static DateTime FromIntToDateTime(this DateTime datetime, int date, int time)
		{
			if (date < 10000000)
				throw new ArgumentOutOfRangeException("Invalid Date");
			int yyyy = date / 10000;
			if (yyyy > DateTime.MaxValue.Year)
				throw new ArgumentOutOfRangeException("Year");
			int mm = date % 10000 / 100;
			int dd = date % 10000 % 100;

			int hh = time / 10000;
			int mi = time % 10000 / 100;
			int ss = time % 10000 % 100;
			return new DateTime(yyyy, mm, dd, hh, mi, ss);
		}

		public static bool HasPast(this DateTime datetime)
		{
			bool result = false;
			var dt = DateTime.Now;
			if(DateTime.Compare(datetime, dt) < 0 || DateTime.Compare(datetime, dt) == 0)
			{
				result = true;
			}
			return result;
		}
		
		public static DateTime SetNumericDate(this DateTime datetime, int date)
		{
			var time = datetime.TimeToInt();
			return datetime.FromIntToDateTime(date, time);
		}

		public static DateTime SetNumericTime(this DateTime datetime, int time)
		{
			var h = time / 10000;
			var m = time % 10000 / 100;
			var s = time % 10000 % 100;
			var ts = new TimeSpan(h, m, s);
			return datetime.Date + ts;
		}

		public static DateTime AddSubInterval(this DateTime datetime, SubIntervalType subtype, int interval)
		{
			DateTime dt = datetime;
			switch (subtype)
			{
				case SubIntervalType.SpecificTime:
					dt = dt.Add(TimeSpan.FromDays(1));
					break;
				case SubIntervalType.Hours:
					dt = dt.Add(TimeSpan.FromHours(interval));
					break;
				case SubIntervalType.Minutes:
					dt = dt.Add(TimeSpan.FromMinutes(interval));
					break;
			}
			return dt;
		}
	}
}
