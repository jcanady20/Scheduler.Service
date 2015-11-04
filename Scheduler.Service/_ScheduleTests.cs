using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Scheduler.Data;
using Scheduler.Data.Entities;
using Scheduler.Data.Extensions;
using Scheduler.Scheduling;

namespace Scheduler.Service
{
	public partial class Program
	{
		static void daily()
		{
			var startDate = new DateTime(2015, 03, 26, 9, 0, 0);
			var endDate = startDate.AddDays(2).AddHours(12);
			var schedule = new JobSchedule();
			schedule.Type = FrequencyType.Daily;
			schedule.Interval = 1;
			schedule.SubdayType = SubIntervalType.Minutes;
			schedule.SubdayInterval = 5;
			schedule.StartDateTime = startDate;
			schedule.EndDateTime = endDate;

			var now = DateTime.Now;
			NextRun(schedule, now);
		}

		static void dailyS()
		{
			var startDate = new DateTime(2015, 02, 26, 8, 0, 0);
			var schedule = new JobSchedule();
			schedule.Type = FrequencyType.Daily;
			schedule.Interval = 1;
			schedule.SubdayType = SubIntervalType.SpecificTime;
			schedule.SubdayInterval = 5;
			schedule.StartDateTime = startDate;
			schedule.EndDateTime = DateTime.MaxValue;

			var now = DateTime.Now.AddHours(12);
			NextRun(schedule, now);
		}

		static void weekly()
		{
			var startDate = new DateTime(2015, 02, 26, 10, 0, 0);
			var schedule = new JobSchedule();
			schedule.Type = FrequencyType.Weekly;
			schedule.Interval = 6;
			schedule.SubdayType = SubIntervalType.SpecificTime;
			schedule.StartDateTime = startDate;
			schedule.EndDateTime = DateTime.MaxValue;

			//var now = DateTime.Now;
			var now = new DateTime(2015, 03, 05, 12, 0, 0);
			NextRun(schedule, now);
		}

		static void monthly()
		{
			var startDate = new DateTime(2015, 02, 26, 10, 0, 0);
			var schedule = new JobSchedule();
			schedule.Type = FrequencyType.Monthly;
			schedule.Interval = 10;
			schedule.SubdayType = SubIntervalType.SpecificTime;
			schedule.RecurrenceFactor = 5;
			schedule.StartDateTime = startDate;
			schedule.EndDateTime = DateTime.MaxValue;

			NextRun(schedule, DateTime.Now);
		}

		static void monthlyR()
		{
			var startDate = new DateTime(2015, 02, 26, 10, 0, 0);
			var schedule = new JobSchedule();
			schedule.Type = FrequencyType.MonthlyRelative;
			schedule.Interval = 10;
			schedule.SubdayType = SubIntervalType.SpecificTime;
			schedule.RecurrenceFactor = 5;
			schedule.StartDateTime = startDate;
			schedule.EndDateTime = DateTime.MaxValue;

			NextRun(schedule, DateTime.Now);
		}

		static void NextRun(JobSchedule schedule, DateTime lastRunTime)
		{
			var nextRunDate = schedule.CalculateNextRun(lastRunTime);
			Console.WriteLine("{0}", schedule.Description());
			Console.WriteLine("Next Run: {0}, {1} {2}:{3}:{4}", nextRunDate.Month, nextRunDate.Day, nextRunDate.Hour, nextRunDate.Minute, nextRunDate.Second);
		}
	}
}
