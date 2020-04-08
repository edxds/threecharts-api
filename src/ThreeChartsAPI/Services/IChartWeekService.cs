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
        Task<ChartWeek> GetChartWeek(int ownerId, int weekId);
        Task<List<ChartWeek>> GetUserChartWeeks(int ownerId);

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

        Task<List<ChartEntry>> CreateEntriesForLastFmCharts(
            LastFmChart<LastFmChartTrack> trackChart,
            LastFmChart<LastFmChartAlbum> albumChart,
            LastFmChart<LastFmChartArtist> artistChart,
            ChartWeek targetChartWeek
        );

        (ChartEntryStat stat, string? statText) GetStatsForChartEntry(
            ChartEntry entry,
            List<ChartWeek> weeks);
    }
}