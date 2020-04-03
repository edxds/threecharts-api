using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using ThreeChartsAPI.Models;
using ThreeChartsAPI.Models.LastFm;

namespace ThreeChartsAPI.Services
{
    public interface IChartWeekService
    {
        List<ChartWeek> GetChartWeeksInDateRange(DateTime startDate, DateTime endDate);

        Task<ChartWeek> GetChartWeek(int ownerId, int weekId);
        Task<List<ChartWeek>> GetUserChartWeeks(int ownerId);
        Task<List<ChartEntry>> CreateEntriesForLastFmCharts(
            LastFmChart<LastFmChartTrack> trackChart,
            LastFmChart<LastFmChartAlbum> albumChart,
            LastFmChart<LastFmChartArtist> artistChart,
            ChartWeek targetChartWeek
        );

        (ChartEntryStat stat, string? statText) GetStatsForChartEntry(ChartEntry entry, List<ChartWeek> weeks);
    }
}