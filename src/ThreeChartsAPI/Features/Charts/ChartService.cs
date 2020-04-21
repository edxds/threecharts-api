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

namespace ThreeChartsAPI.Features.Charts
{
    public class ChartService : IChartService
    {
        private readonly ThreeChartsContext _context;
        private readonly IChartDateService _chartDateService;
        private readonly ILastFmService _lastFm;

        public ChartService(
            ThreeChartsContext context,
            IChartDateService chartDateService,
            ILastFmService lastFmService)
        {
            _context = context;
            _chartDateService = chartDateService;
            _lastFm = lastFmService;
        }

        public Task<ChartWeek> GetChartWeek(int ownerId, int weekId)
        {
            return _context.ChartWeeks
                .Include(week => week.ChartEntries)
                    .ThenInclude(entry => entry.Artist)
                .Include(week => week.ChartEntries)
                    .ThenInclude(entry => entry.Album)
                .Include(week => week.ChartEntries)
                    .ThenInclude(entry => entry.Track)
                .Where(week => week.OwnerId == ownerId && week.Id == weekId)
                .FirstOrDefaultAsync();
        }
        
        public Task<List<ChartWeek>> GetUserChartWeeks(int ownerId)
        {
            return _context.ChartWeeks.Where(week => week.OwnerId == ownerId).ToListAsync();
        }
        
        public async Task<Result> SyncWeeks(
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
                week.ChartEntries = await CreateEntriesForLastFmCharts(
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
                var (stat, statText) = GetStatsForChartEntry(entry, previousWeeks);
                entry.Stat = stat;
                entry.StatText = statText;
            });

            await _context.ChartWeeks.AddRangeAsync(newWeeks);
            await _context.SaveChangesAsync();

            return Results.Ok();
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
                if (entry.Rank <= 100)
                {
                    var track = await GetTrackOrCreate(entry.Artist, entry.Title);
                    entries.Add(new ChartEntry()
                    {
                        Week = targetWeek,
                        Type = ChartEntryType.Track,
                        Rank = entry.Rank,
                        Track = track,
                    });
                }
            }

            foreach (var entry in albumChart.Entries)
            {
                if (entry.Rank <= 100)
                {
                    var album = await GetAlbumOrCreate(entry.Artist, entry.Title);
                    entries.Add(new ChartEntry()
                    {
                        Week = targetWeek,
                        Type = ChartEntryType.Album,
                        Rank = entry.Rank,
                        Album = album,
                    });
                }
            }

            foreach (var entry in artistChart.Entries)
            {
                if (entry.Rank <= 100)
                {
                    var artist = await GetArtistOrCreate(entry.Name);
                    entries.Add(new ChartEntry()
                    {
                        Week = targetWeek,
                        Type = ChartEntryType.Artist,
                        Rank = entry.Rank,
                        Artist = artist,
                    });
                }
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

        // TODO: Move to generic DbSet extension
        private async Task<Track> GetTrackOrCreate(string artist, string title)
        {
            var track = await _context.Tracks
                .FirstOrDefaultAsync(track => track.ArtistName == artist && track.Title == title);

            if (track == null)
            {
                track = new Track()
                {
                    ArtistName = artist,
                    Title = title
                };
            }

            return track;
        }

        private async Task<Album> GetAlbumOrCreate(string artist, string title)
        {
            var album = await _context.Albums
                .FirstOrDefaultAsync(album => album.ArtistName == artist && album.Title == title);

            if (album == null)
            {
                album = new Album()
                {
                    ArtistName = artist,
                    Title = title
                };
            }

            return album;
        }

        private async Task<Artist> GetArtistOrCreate(string name)
        {
            var artist = await _context.Artists.FirstOrDefaultAsync(artist => artist.Name == name);

            if (artist == null)
            {
                artist = new Artist()
                {
                    Name = name,
                };
            }

            return artist;
        }
    }
}