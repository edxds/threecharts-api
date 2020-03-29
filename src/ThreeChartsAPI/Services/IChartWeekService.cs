using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThreeChartsAPI.Models;

namespace ThreeChartsAPI.Services
{
    public interface IChartWeekService
    {
        List<ChartWeek> GetChartWeeksInDateRange(DateTime startDate, DateTime endDate);
        Task<List<ChartEntry>> CreateEntriesForChartWeek(ChartWeek chartWeek);
    }
}