using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using ThreeChartsAPI.Features.Charts.Models;
using ThreeChartsAPI.Features.LastFm.Models;
using ThreeChartsAPI.Features.Users.Models;

namespace ThreeChartsAPI.Features.Charts
{
    public interface IChartService
    {
        Task<Result<ChartWeek>> GetLiveWeekFor(
            User user,
            DateTime currentTime,
            CancellationToken? cancellationToken = null);

        Task<Result<List<ChartWeek>>> SyncWeeks(
            User user,
            int startWeekNumber,
            DateTime startDate,
            DateTime? endDate,
            TimeZoneInfo timeZone,
            CancellationToken? cancellationToken = null);
        
        List<ChartEntry> CreateEntriesForLastFmCharts(
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