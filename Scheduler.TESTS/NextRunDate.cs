using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scheduler.Data;
using Scheduler.Data.Entities;
using Scheduler.Data.Extensions;


namespace Scheduler.TESTS
{
	[TestClass]
	public class NextRunDate
	{
        public void daily()
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

            var lastRunDate = DateTime.Now;
            var nextRunDate = schedule.CalculateNextRun(lastRunDate);
        }

        public void dailyS()
        {
            var startDate = new DateTime(2015, 02, 26, 8, 0, 0);
            var schedule = new JobSchedule();
            schedule.Type = FrequencyType.Daily;
            schedule.Interval = 1;
            schedule.SubdayType = SubIntervalType.SpecificTime;
            schedule.SubdayInterval = 5;
            schedule.StartDateTime = startDate;
            schedule.EndDateTime = DateTime.MaxValue;

            var lastRunDate = DateTime.Now.AddHours(12);
            var nextRunDate = schedule.CalculateNextRun(lastRunDate);
        }

        public void weekly()
        {
            var startDate = new DateTime(2015, 02, 26, 10, 0, 0);
            var schedule = new JobSchedule();
            schedule.Type = FrequencyType.Weekly;
            schedule.Interval = 6;
            schedule.SubdayType = SubIntervalType.SpecificTime;
            schedule.StartDateTime = startDate;
            schedule.EndDateTime = DateTime.MaxValue;

            //var now = DateTime.Now;
            var lastRunDate = new DateTime(2015, 03, 05, 12, 0, 0);
            var nextRunDate = schedule.CalculateNextRun(lastRunDate);
        }

        public void monthly()
        {
            var startDate = new DateTime(2015, 02, 26, 10, 0, 0);
            var schedule = new JobSchedule();
            schedule.Type = FrequencyType.Monthly;
            schedule.Interval = 10;
            schedule.SubdayType = SubIntervalType.SpecificTime;
            schedule.RecurrenceFactor = 5;
            schedule.StartDateTime = startDate;
            schedule.EndDateTime = DateTime.MaxValue;

            var lastRunDate = DateTime.Now;
            var nextRunDate = schedule.CalculateNextRun(lastRunDate);
        }

        public void monthlyR()
        {
            var startDate = new DateTime(2015, 02, 26, 10, 0, 0);
            var schedule = new JobSchedule();
            schedule.Type = FrequencyType.MonthlyRelative;
            schedule.Interval = 10;
            schedule.SubdayType = SubIntervalType.SpecificTime;
            schedule.RecurrenceFactor = 5;
            schedule.StartDateTime = startDate;
            schedule.EndDateTime = DateTime.MaxValue;

            var lastRunDate = DateTime.Now;
            var nextRunDate = schedule.CalculateNextRun(lastRunDate);
        }
    }
}
