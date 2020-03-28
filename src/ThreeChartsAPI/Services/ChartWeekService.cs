using System;
using System.Collections.Generic;
using ThreeChartsAPI.Models;

namespace ThreeChartsAPI.Services
{
    public class ChartWeekService : IChartWeekService
    {
        public List<ChartWeek> GetChartWeeksInDateRange(DateTime startDate, DateTime endDate)
        {
            var chartWeekList = new List<ChartWeek>();

            var startDateWithoutTime = new DateTime(
                startDate.Year,
                startDate.Month,
                startDate.Day
            );

            var daysUntilFriday = DayOfWeek.Friday - startDateWithoutTime.DayOfWeek;
            var ticksUntilFriday = TimeSpan.TicksPerDay * daysUntilFriday;

            var firstChartStartDate = new DateTime(startDateWithoutTime.Ticks + ticksUntilFriday);
            var firstChartEndDate = GetChartEndDateForStartDate(firstChartStartDate);

            if (firstChartEndDate > endDate)
            {
                return chartWeekList;
            }

            var currentChartStartDate = firstChartStartDate;
            var currentChartEndDate = firstChartEndDate;
            var currentWeekNumber = 1;

            while (currentChartEndDate < endDate)
            {
                chartWeekList.Add(new ChartWeek()
                {
                    WeekNumber = currentWeekNumber,
                    From = currentChartStartDate,
                    To = currentChartEndDate,
                });

                currentWeekNumber++;
                currentChartStartDate = currentChartEndDate.AddSeconds(1);
                currentChartEndDate = GetChartEndDateForStartDate(currentChartStartDate);
            }

            return chartWeekList;
        }

        private DateTime GetChartEndDateForStartDate(DateTime chartDate)
        {
            return chartDate
                    .AddDays(6)
                    .AddHours(23)
                    .AddMinutes(59)
                    .AddSeconds(59);
        }
    }
}