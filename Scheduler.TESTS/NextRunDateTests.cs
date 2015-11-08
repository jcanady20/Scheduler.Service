using System;
using Scheduler.Data;
using Scheduler.Data.Entities;
using Scheduler.Data.Extensions;
using Xunit;

namespace Scheduler.TESTS
{
	public class NextRunDateTests
	{
        public void Setup()
        { }

        [Fact(DisplayName = "Next Rune Date: Daily")]
        public void daily()
        {
            var startDate = new DateTime(2015, 03, 26, 9, 0, 0);
            var endDate = startDate.AddDays(2).AddHours(12);
            var sut = new Schedule();
            sut.Type = FrequencyType.Daily;
            sut.Interval = 1;
            sut.SubdayType = SubIntervalType.Minutes;
            sut.SubdayInterval = 5;
            sut.StartDate = startDate.Date;
            sut.StartTime = startDate.TimeOfDay;
            sut.EndDate = endDate.Date;
            sut.EndTime = endDate.TimeOfDay;

            var lastRunDate = DateTime.Now;
            var result = sut.CalculateNextRun(lastRunDate);
            var exp = sut.MaxDateTime();
            Assert.Equal(exp, result);
        }

        [Fact(DisplayName = "Next Rune Date: Daily.Specified Time")]
        public void dailyS()
        {
            var hour = 8;
            var minute = 0;
            var second = 0;
            var startDate = new DateTime(2015, 02, 26, hour, minute, second);
            var sut = new Schedule();
            sut.Type = FrequencyType.Daily;
            sut.Interval = 1;
            sut.SubdayType = SubIntervalType.SpecificTime;
            sut.SubdayInterval = 5;
            sut.StartDate = startDate.Date;
            sut.StartTime = startDate.TimeOfDay;
            sut.EndDate = DateTime.MaxValue.Date;
            sut.EndTime = DateTime.MaxValue.TimeOfDay;

            var dtn = DateTime.Now;
            var tmw = dtn.AddDays(1);
            var lrd = new DateTime(dtn.Year, dtn.Month, dtn.Day, hour, minute, second);
            var exp = new DateTime(tmw.Year, tmw.Month, tmw.Day, hour, minute, second);
            var result = sut.CalculateNextRun(lrd);
            Assert.Equal(exp, result);
        }

        [Fact(DisplayName = "Next Rune Date: Weekly")]
        public void weekly()
        {
            var startDate = new DateTime(2015, 02, 26, 10, 0, 0);
            var sut = new Schedule();
            sut.Type = FrequencyType.Weekly;
            sut.Interval = 6;
            sut.SubdayType = SubIntervalType.SpecificTime;
            sut.StartDate = startDate.Date;
            sut.StartTime = startDate.TimeOfDay;
            sut.EndDate = DateTime.MaxValue.Date;
            sut.EndTime = DateTime.MaxValue.TimeOfDay;

            //var now = DateTime.Now;
            var lastRunDate = new DateTime(2015, 03, 05, 12, 0, 0);
            var result = sut.CalculateNextRun(lastRunDate);
        }

        [Fact(DisplayName = "Next Rune Date: Monthly")]
        public void monthly()
        {
            var dayOftheMonth = 10;
            var hour = 10;
            var minute = 0;
            var second = 0;
            var startDate = new DateTime(2015, 02, 26, hour, minute, second);
            var sut = new Schedule();
            sut.Type = FrequencyType.Monthly;
            sut.Interval = dayOftheMonth;
            sut.SubdayType = SubIntervalType.SpecificTime;
            sut.RecurrenceFactor = 5;
            sut.StartDate = startDate.Date;
            sut.StartTime = startDate.TimeOfDay;
            sut.EndDate = DateTime.MaxValue.Date;
            sut.EndTime = DateTime.MaxValue.TimeOfDay;

            var dtn = DateTime.Now;
            var tmw = dtn.AddMonths(5);
            var lrd = new DateTime(dtn.Year, dtn.Month, dtn.Day, hour, minute, second);
            var exp = new DateTime(tmw.Year, tmw.Month, dayOftheMonth, hour, minute, second);
            var result = sut.CalculateNextRun(lrd);
            Assert.Equal(exp, result);
        }

        [Fact(DisplayName = "Next Rune Date: Monthly Relative")]
        public void monthlyR()
        {
            var startDate = new DateTime(2015, 02, 26, 10, 0, 0);
            var sut = new Schedule();
            sut.Type = FrequencyType.MonthlyRelative;
            sut.Interval = 10;
            sut.SubdayType = SubIntervalType.SpecificTime;
            sut.RecurrenceFactor = 5;
            sut.StartDate = startDate.Date;
            sut.StartTime = startDate.TimeOfDay;
            sut.EndDate = DateTime.MaxValue.Date;
            sut.EndTime = DateTime.MaxValue.TimeOfDay;

            var lastRunDate = DateTime.Now;
            var nextRunDate = sut.CalculateNextRun(lastRunDate);
        }
    }
}
