using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using ThreeChartsAPI.Models;

namespace ThreeChartsAPI.Services
{
    public interface IChartWeekService
    {
        List<ChartWeek> GetChartWeeksInDateRange(DateTime startDate, DateTime endDate);
        Task<Result<List<ChartEntry>>> CreateEntriesForChartWeek(ChartWeek chartWeek);
        (ChartEntryStat stat, string? statText) GetStatsForChartEntry(ChartEntry entry, List<ChartWeek> weeks);
    }
}