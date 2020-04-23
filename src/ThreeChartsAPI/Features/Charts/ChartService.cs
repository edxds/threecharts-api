using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using ThreeChartsAPI.Features.Charts.Models;
using ThreeChartsAPI.Features.LastFm;
using ThreeChartsAPI.Features.LastFm.Models;
using ThreeChartsAPI.Features.Users.Models;
using TimeZoneConverter;

namespace ThreeChartsAPI.Features.Charts
{
    public class ChartService : IChartService
    {
        private readonly ChartRepository _repo;
        private readonly IChartDateService _chartDateService;
        private readonly ILastFmService _lastFm;

        public ChartService(
            ChartRepository repo,
            IChartDateService chartDateService,
            ILastFmService lastFmService)
        {
            _repo = repo;
            _chartDateService = chartDateService;
            _lastFm = lastFmService;
        }

        public async Task<Result<ChartWeek>> GetLiveWeekFor(User user, DateTime currentTime)
        {
            var timezone = TZConvert.GetTimeZoneInfo(user.IanaTimezone ?? "");
            if (timezone == null)
            {
                throw new InvalidOperationException();
            }

            var lastUserWeek = await _repo.QueryWeeksOf(user.Id)
                .OrderByDescending(week => week.WeekNumber)
                .FirstOrDefaultAsync();

            var fromDate = lastUserWeek?.To.AddSeconds(1) ?? user.RegisteredAt;
            var liveWeek = _chartDateService
                .GetChartWeeksInDateRange(lastUserWeek?.WeekNumber + 1 ?? 1,
                    fromDate, currentTime, timezone, true)
                .LastOrDefault();

            var fromUnix = ToUnixTimeSeconds(liveWeek.From);
            var toUnix = ToUnixTimeSeconds(liveWeek.To);

            var artistChart = _lastFm.GetWeeklyArtistChart(user.UserName, fromUnix, toUnix);
            var albumChart = _lastFm.GetWeeklyAlbumChart(user.UserName, fromUnix, toUnix);
            var trackChart = _lastFm.GetWeeklyTrackChart(user.UserName, fromUnix, toUnix);

            // Allows for parallel running since Tasks are started at call, not at await
            await artistChart;
            await albumChart;
            await trackChart;

            var mergedResult =
                Results.Merge(artistChart.Result, albumChart.Result, trackChart.Result);
            if (mergedResult.IsFailed)
            {
                return mergedResult;
            }

            liveWeek.Owner = user;
            liveWeek.ChartEntries = await CreateEntriesForLastFmCharts(trackChart.Result.Value, albumChart.Result.Value,
                artistChart.Result.Value, liveWeek);

            await _repo.AddMusicalEntitiesFromWeekAndSaveChanges(liveWeek);
            var existingWeeks = await _repo.QueryWeeksWithRelationsOf(user.Id)
                .ToListAsync();
            
            liveWeek.ChartEntries.ForEach(entry =>
            {
                var (stat, statText) = GetStatsForChartEntry(entry, existingWeeks);
                entry.Stat = stat;
                entry.StatText = statText;
            });

            return Results.Ok(liveWeek);
        }

        public async Task<Result<List<ChartWeek>>> SyncWeeks(
            User user,
            int startWeekNumber,
            DateTime startDate,
            DateTime? endDate,
            TimeZoneInfo timeZone)
        {
            var newWeeks = _chartDateService.GetChartWeeksInDateRange(
                startWeekNumber,
                startDate,
                endDate ?? DateTime.UtcNow,
                timeZone
            );

            var trackChartTasks = newWeeks
                .Select(week => _lastFm.GetWeeklyTrackChart(
                    user.UserName,
                    ToUnixTimeSeconds(week.From),
                    ToUnixTimeSeconds(week.To)))
                .ToList();

            var albumChartTasks = newWeeks
                .Select(week => _lastFm.GetWeeklyAlbumChart(
                    user.UserName,
                    ToUnixTimeSeconds(week.From),
                    ToUnixTimeSeconds(week.To)))
                .ToList();

            var artistChartTasks = newWeeks
                .Select(week => _lastFm.GetWeeklyArtistChart(
                    user.UserName,
                    ToUnixTimeSeconds(week.From),
                    ToUnixTimeSeconds(week.To)))
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
                week.ChartEntries = await CreateEntriesForLastFmCharts(
                    trackChart.Value,
                    albumChart.Value,
                    artistChart.Value,
                    week
                );
            }

            var entries = newWeeks.SelectMany(week => week.ChartEntries).ToList();

            var existingWeeks = await _repo.QueryWeeksWithRelationsOf(user.Id).ToListAsync();
            var allWeeks = existingWeeks.Concat(newWeeks).ToList();

            entries.ForEach(entry =>
            {
                var (stat, statText) = GetStatsForChartEntry(entry, allWeeks);
                entry.Stat = stat;
                entry.StatText = statText;
            });

            await _repo.AddWeeksAndSaveChanges(newWeeks);
            return Results.Ok(allWeeks);
        }
        
        public async Task<List<ChartEntry>> CreateEntriesForLastFmCharts(
            LastFmChart<LastFmChartTrack> trackChart,
            LastFmChart<LastFmChartAlbum> albumChart,
            LastFmChart<LastFmChartArtist> artistChart,
            ChartWeek targetWeek)
        {
            var entries = new List<ChartEntry>();

            foreach (var entry in trackChart.Entries)
            {
                if (entry.Rank > 100) continue;
                
                var track = await _repo.GetTrackOrCreate(entry.Artist, entry.Title);
                entries.Add(new ChartEntry()
                {
                    Week = targetWeek,
                    Type = ChartEntryType.Track,
                    Rank = entry.Rank,
                    Track = track,
                });
            }

            foreach (var entry in albumChart.Entries)
            {
                if (entry.Rank > 100) continue;
                
                var album = await _repo.GetAlbumOrCreate(entry.Artist, entry.Title);
                entries.Add(new ChartEntry()
                {
                    Week = targetWeek,
                    Type = ChartEntryType.Album,
                    Rank = entry.Rank,
                    Album = album,
                });
            }

            foreach (var entry in artistChart.Entries)
            {
                if (entry.Rank > 100) continue;
                
                var artist = await _repo.GetArtistOrCreate(entry.Name);
                entries.Add(new ChartEntry()
                {
                    Week = targetWeek,
                    Type = ChartEntryType.Artist,
                    Rank = entry.Rank,
                    Artist = artist,
                });
            }

            return entries;
        }

        public (ChartEntryStat stat, string? statText) GetStatsForChartEntry(ChartEntry entry, List<ChartWeek> weeks)
        {
            if (entry.Week.WeekNumber < 1)
            {
                throw new ArgumentOutOfRangeException(
                    "entry.Week.WeekNumber",
                    entry.Week.WeekNumber,
                    "The entry's week number shouldn't be lower than 1!"
                );
            }

            var isFirstWeek = entry.Week.WeekNumber == 1;
            if (isFirstWeek)
            {
                return (stat: ChartEntryStat.New, statText: null);
            }

            var previousWeek = weeks
                .Where(week => week.WeekNumber == entry.Week.WeekNumber - 1)
                .FirstOrDefault();

            var entryOnPreviousWeek = previousWeek.ChartEntries
                .Find(prevEntry =>
                    (prevEntry.Track?.Title == entry.Track?.Title &&
                        prevEntry.Track?.ArtistName == entry.Track?.ArtistName) &&
                    (prevEntry.Album?.Title == entry.Album?.Title &&
                        prevEntry.Album?.ArtistName == entry.Album?.ArtistName) &&
                    (prevEntry.Artist?.Name == entry.Artist?.Name));

            if (entryOnPreviousWeek != null)
            {
                var currentRank = entry.Rank;
                var previousRank = entryOnPreviousWeek.Rank;

                var difference = previousRank - currentRank;
                if (difference == 0)
                {
                    return (stat: ChartEntryStat.NoDiff, statText: "=");
                }

                var isDrop = difference < 0;
                if (isDrop)
                {
                    return (stat: ChartEntryStat.Decrease, statText: $"-{Math.Abs(difference)}");
                }

                return (stat: ChartEntryStat.Increase, statText: $"+{Math.Abs(difference)}");
            }

            var previousEntry = weeks
                .Where(week => week.WeekNumber < entry.Week.WeekNumber)
                .SelectMany(week => week.ChartEntries)
                .ToList()
                .Find(prevEntry =>
                    (prevEntry.Track?.Title == entry.Track?.Title &&
                        prevEntry.Track?.ArtistName == entry.Track?.ArtistName) &&
                    (prevEntry.Album?.Title == entry.Album?.Title &&
                        prevEntry.Album?.ArtistName == entry.Album?.ArtistName) &&
                    (prevEntry.Artist?.Name == entry.Artist?.Name));

            if (previousEntry != null)
            {
                return (stat: ChartEntryStat.Reentry, statText: null);
            }

            return (stat: ChartEntryStat.New, statText: null);
        }

        private long ToUnixTimeSeconds(DateTime dateTime) =>
            new DateTimeOffset(dateTime).ToUnixTimeSeconds();
    }
}