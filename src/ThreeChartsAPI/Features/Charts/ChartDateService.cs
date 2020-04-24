using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThreeChartsAPI.Features.Charts.Models;
using ThreeChartsAPI.Features.LastFm.Models;

namespace ThreeChartsAPI.Features.Charts
{
    public class ChartDateService : IChartDateService
    {
        private readonly ThreeChartsContext _context;

        public ChartDateService(ThreeChartsContext context)
        {
            _context = context;
        }

        public List<ChartWeek> GetChartWeeksInDateRange(
            int startWeekNumber,
            DateTime startDate,
            DateTime endDate,
            TimeZoneInfo timeZone,
            bool toleratesInitialDateOverflow = false)
        {
            var chartWeekList = new List<ChartWeek>();

            var startDateAtMidnightUnspecified = DateTime.SpecifyKind(new DateTime(
                startDate.Year,
                startDate.Month,
                startDate.Day), DateTimeKind.Unspecified);

            var startDateMidnightUtc = TimeZoneInfo.ConvertTimeToUtc(
                startDateAtMidnightUnspecified,
                timeZone);

            var firstChartStartDate = startDateMidnightUtc;
            var firstChartEndDate = GetChartEndDateForStartDate(firstChartStartDate);

            if (!toleratesInitialDateOverflow && firstChartEndDate > endDate)
            {
                return chartWeekList;
            }

            var currentChartStartDate = firstChartStartDate;
            var currentChartEndDate = firstChartEndDate;
            var currentWeekNumber = startWeekNumber;

            do
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
            } while (currentChartEndDate < endDate);

            // 26 is roughly six months
            return chartWeekList.TakeLast(26).ToList();
        }

        public async Task<List<ChartWeek>> GetOutdatedWeeks(
            int ownerId,
            DateTime defaultStartDate,
            DateTime endDate,
            TimeZoneInfo timeZone)
        {
            var lastWeek = await _context.ChartWeeks.Where(week => week.OwnerId == ownerId)
                .OrderByDescending(week => week.WeekNumber)
                .FirstOrDefaultAsync();

            return GetChartWeeksInDateRange(
                lastWeek?.WeekNumber + 1 ?? 1,
                lastWeek?.To.AddSeconds(1) ?? defaultStartDate,
                endDate,
                timeZone);
        }
        
        private DateTime GetChartEndDateForStartDate(DateTime chartDate)
        {
            var daysUntilFriday = DayOfWeek.Friday - chartDate.DayOfWeek;
            
            // Next friday!
            if (daysUntilFriday == 0)
            {
                daysUntilFriday = 7;
            }
            
            var oneSecond = new TimeSpan(0, 0, 1);
            return chartDate.AddDays(daysUntilFriday).Subtract(oneSecond);
        }
    }
}