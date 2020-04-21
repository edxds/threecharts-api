using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using ThreeChartsAPI.Features.LastFm;
using ThreeChartsAPI.Features.Users.Models;

namespace ThreeChartsAPI.Features.Charts
{
    public class ChartService : IChartService
    {
        private readonly ThreeChartsContext _context;
        private readonly IChartWeekService _chartWeekService;
        private readonly ILastFmService _lastFm;

        public ChartService(
            ThreeChartsContext context,
            IChartWeekService chartWeekService,
            ILastFmService lastFmService)
        {
            _context = context;
            _chartWeekService = chartWeekService;
            _lastFm = lastFmService;
        }

        public async Task<Result> SyncWeeks(
            User user,
            int startWeekNumber,
            DateTime startDate,
            DateTime? endDate,
            TimeZoneInfo timeZone)
        {
            var newWeeks = _chartWeekService.GetChartWeeksInDateRange(
                startWeekNumber,
                startDate,
                endDate ?? DateTime.UtcNow,
                timeZone
            );

            var trackChartTasks = newWeeks
                .Select(week => _lastFm.GetWeeklyTrackChart(
                    user.UserName,
                    new DateTimeOffset(week.From).ToUnixTimeSeconds(),
                    new DateTimeOffset(week.To).ToUnixTimeSeconds()))
                .ToList();

            var albumChartTasks = newWeeks
                .Select(week => _lastFm.GetWeeklyAlbumChart(
                    user.UserName,
                    new DateTimeOffset(week.From).ToUnixTimeSeconds(),
                    new DateTimeOffset(week.To).ToUnixTimeSeconds()))
                .ToList();

            var artistChartTasks = newWeeks
                .Select(week => _lastFm.GetWeeklyArtistChart(
                    user.UserName,
                    new DateTimeOffset(week.From).ToUnixTimeSeconds(),
                    new DateTimeOffset(week.To).ToUnixTimeSeconds()))
                .ToList();

            await Task.WhenAll(trackChartTasks);
            await Task.WhenAll(albumChartTasks);
            await Task.WhenAll(artistChartTasks);

            if (newWeeks.Count != trackChartTasks.Count() ||
                trackChartTasks.Count() != albumChartTasks.Count() ||
                trackChartTasks.Count() != artistChartTasks.Count() ||
                artistChartTasks.Count() != albumChartTasks.Count())
            {
                throw new InvalidOperationException("Chart counts don't match!");
            }

            for (int i = 0; i < newWeeks.Count(); i++)
            {
                var week = newWeeks[i];
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

            var entries = newWeeks.SelectMany(week => week.ChartEntries).ToList();

            var savedWeeks = await _context.ChartWeeks
                .Where(week => week.OwnerId == user.Id)
                .OrderByDescending(week => week.WeekNumber)
                .Include(week => week.ChartEntries)
                    .ThenInclude(entry => entry.Track)
                .Include(week => week.ChartEntries)
                    .ThenInclude(entry => entry.Album)
                .Include(week => week.ChartEntries)
                    .ThenInclude(entry => entry.Artist)
                .ToListAsync();

            var previousWeeks = savedWeeks.Concat(newWeeks).ToList();

            entries.ForEach(entry =>
            {
                var (stat, statText) = _chartWeekService.GetStatsForChartEntry(entry, previousWeeks);
                entry.Stat = stat;
                entry.StatText = statText;
            });

            await _context.ChartWeeks.AddRangeAsync(newWeeks);
            await _context.SaveChangesAsync();

            return Results.Ok();
        }
    }
}