using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using ThreeChartsAPI.Features.Charts.Models;
using ThreeChartsAPI.Features.LastFm.Models;

namespace ThreeChartsAPI.Features.Charts
{
    public interface IChartDateService
    {
        List<ChartWeek> GetChartWeeksInDateRange(
            int startWeekNumber,
            DateTime startDate,
            DateTime endDate,
            TimeZoneInfo timeZone);

        Task<List<ChartWeek>> GetOutdatedWeeks(
            int ownerId,
            DateTime defaultStartDate,
            DateTime endDate,
            TimeZoneInfo timeZone);
    }
}