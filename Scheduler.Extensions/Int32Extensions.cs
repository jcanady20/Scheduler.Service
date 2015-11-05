using System;
using System.Globalization;

namespace Scheduler.Extensions
{
	public static class Int32Extensions
	{
		public static bool Between(this int value, int min, int max)
		{
			if (value >= min && value <= max)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static DateTime ToDate(this int dateInt)
		{
			return DateTime.ParseExact(dateInt.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);
		}

		public static DateTime? ToDate(this int? dateInt)
		{
			if (dateInt.HasValue)
			{
				return dateInt.Value.ToDate();
			}
			else
			{
				return (DateTime?)null;
			}
		}

		public static int GetAgeInMonths(this int? dateInt, DateTime reportDate)
		{
			int ageInMonths = 0;

			if (dateInt.HasValue)
			{
				DateTime date = dateInt.ToDate().Value;
				if (reportDate < date)
				{
					// Keeping this logic from Registry 6.1
					// WICKED TODO: Bug fix, this should evaluate as negative age
					ageInMonths = 0;
				}
				else
				{
					int years = reportDate.Year - date.Year;
					ageInMonths = (years * 12) + (reportDate.Month - date.Month);

					// Adjust the months if the birthday has not occurred yet in the current month.
					if (reportDate.Day < date.Day)
					{
						ageInMonths--;
					}
				}
			}
			else
			{
				// WICKED TODO: BUG MATCHES REG 6.1
				// Unknown birthdate results in age of 0 months
				ageInMonths = 0;
			}

			return ageInMonths;
		}

		public static int GetAgeInWeeks(this int? dateInt, DateTime reportDate)
		{
			int ageInWeeks = 0;

			if (dateInt.HasValue)
			{
				DateTime date = dateInt.ToDate().Value;
				if (reportDate < date)
				{
					// Keeping this logic from Registry 6.1
					// WICKED TODO: Bug fix, this should evaluate as negative age
					ageInWeeks = 0;
				}
				else
				{
					ageInWeeks = (reportDate.Date - date.Date).Days / 7;
				}
			}
			else
			{
				// WICKED TODO: BUG MATCHES REG 6.1
				// Unknown birthdate results in age of 0 weeks
				ageInWeeks = 0;
			}

			return ageInWeeks;
		}
	}
}
