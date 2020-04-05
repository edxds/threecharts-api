using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentResults;
using ThreeChartsAPI.Models;
using ThreeChartsAPI.Services.LastFm;

namespace ThreeChartsAPI.Services.Onboarding
{
    public class OnboardingService : IOnboardingService
    {
        private readonly ThreeChartsContext _context;
        private readonly IChartWeekService _chartWeekService;
        private readonly ILastFmService _lastFm;

        public OnboardingService(
            ThreeChartsContext context,
            IChartWeekService chartWeekService,
            ILastFmService lastFmService)
        {
            _context = context;
            _chartWeekService = chartWeekService;
            _lastFm = lastFmService;
        }

        public async Task<Result> SyncWeeks(User user, DateTime startDate, DateTime? endDate)
        {
            var weeks = _chartWeekService.GetChartWeeksInDateRange(
                startDate,
                endDate ?? DateTime.Now
            );

            var trackChartTasks = weeks.Select(week =>
            {
                var from = new DateTimeOffset(week.From).ToUnixTimeSeconds();
                var to = new DateTimeOffset(week.To).ToUnixTimeSeconds();

                return _lastFm.GetWeeklyTrackChart(user.UserName, from, to);
            }).ToList();

            var albumChartTasks = weeks.Select(week =>
            {
                var from = new DateTimeOffset(week.From).ToUnixTimeSeconds();
                var to = new DateTimeOffset(week.To).ToUnixTimeSeconds();

                return _lastFm.GetWeeklyAlbumChart(user.UserName, from, to);
            }).ToList();

            var artistChartTasks = weeks.Select(week =>
            {
                var from = new DateTimeOffset(week.From).ToUnixTimeSeconds();
                var to = new DateTimeOffset(week.To).ToUnixTimeSeconds();

                return _lastFm.GetWeeklyArtistChart(user.UserName, from, to);
            }).ToList();

            await Task.WhenAll(
                Task.WhenAll(trackChartTasks),
                Task.WhenAll(albumChartTasks),
                Task.WhenAll(artistChartTasks)
            );

            if (weeks.Count != trackChartTasks.Count() ||
                trackChartTasks.Count() != albumChartTasks.Count() ||
                trackChartTasks.Count() != artistChartTasks.Count() ||
                artistChartTasks.Count() != albumChartTasks.Count())
            {
                throw new InvalidOperationException("Chart counts don't match!");
            }

            for (int i = 0; i < weeks.Count(); i++)
            {
                var week = weeks[i];
                var trackChart = trackChartTasks[i].Result;
                var albumChart = albumChartTasks[i].Result;
                var artistChart = artistChartTasks[i].Result;

                var mergedResults = Results.Merge(trackChart, albumChart, artistChart);
                if (mergedResults.IsFailed)
                {
                    return mergedResults;
                }

                week.Owner = user;
                week.ChartEntries = await _chartWeekService.CreateEntriesForLastFmCharts(
                    trackChart.Value,
                    albumChart.Value,
                    artistChart.Value,
                    week
                );
            }

            var entries = weeks.SelectMany(week => week.ChartEntries).ToList();
            entries.ForEach(entry =>
            {
                var (stat, statText) = _chartWeekService.GetStatsForChartEntry(entry, weeks);
                entry.Stat = stat;
                entry.StatText = statText;
            });

            await _context.ChartWeeks.AddRangeAsync(weeks);
            await _context.SaveChangesAsync();

            return Results.Ok();
        }
    }
}