using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThreeChartsAPI.Models;
using ThreeChartsAPI.Services.LastFm;

namespace ThreeChartsAPI.Services
{
    public class ChartWeekService : IChartWeekService
    {
        private readonly ThreeChartsContext _context;
        private readonly ILastFmService _lastFm;

        public ChartWeekService(ThreeChartsContext context, ILastFmService lastFmService)
        {
            _context = context;
            _lastFm = lastFmService;
        }

        public List<ChartWeek> GetChartWeeksInDateRange(DateTime startDate, DateTime endDate)
        {
            var chartWeekList = new List<ChartWeek>();

            var startDateWithoutTime = new DateTime(
                startDate.Year,
                startDate.Month,
                startDate.Day
            );

            var daysUntilFriday = DayOfWeek.Friday - startDateWithoutTime.DayOfWeek;
            var ticksUntilFriday = TimeSpan.TicksPerDay * daysUntilFriday;

            var firstChartStartDate = new DateTime(startDateWithoutTime.Ticks + ticksUntilFriday);
            var firstChartEndDate = GetChartEndDateForStartDate(firstChartStartDate);

            if (firstChartEndDate > endDate)
            {
                return chartWeekList;
            }

            var currentChartStartDate = firstChartStartDate;
            var currentChartEndDate = firstChartEndDate;
            var currentWeekNumber = 1;

            while (currentChartEndDate < endDate)
            {
                chartWeekList.Add(new ChartWeek()
                {
                    WeekNumber = currentWeekNumber,
                    From = currentChartStartDate,
                    To = currentChartEndDate,
                });

                currentWeekNumber++;
                currentChartStartDate = currentChartEndDate.AddSeconds(1);
                currentChartEndDate = GetChartEndDateForStartDate(currentChartStartDate);
            }

            return chartWeekList;
        }

        public async Task<List<ChartEntry>> CreateEntriesForChartWeek(ChartWeek chartWeek)
        {
            var user = chartWeek.Owner.UserName;
            var from = ((DateTimeOffset)chartWeek.From).ToUnixTimeMilliseconds();
            var to = ((DateTimeOffset)chartWeek.To).ToUnixTimeMilliseconds();

            var trackChartTask = _lastFm.GetWeeklyTrackChart(user, from, to);
            var albumChartTask = _lastFm.GetWeeklyAlbumChart(user, from, to);
            var artistChartTask = _lastFm.GetWeeklyArtistChart(user, from, to);

            await Task.WhenAll(trackChartTask, albumChartTask, artistChartTask);

            var trackChart = trackChartTask.Result;
            var albumChart = albumChartTask.Result;
            var artistChart = artistChartTask.Result;

            var trackEntries = trackChart.Entries
                .Select(async lastFmEntry => new ChartEntry()
                {
                    Week = chartWeek,
                    Type = ChartEntryType.Track,
                    Rank = lastFmEntry.Rank,
                    Track = await GetTrackOrCreate(lastFmEntry.Artist, lastFmEntry.Title),
                })
                .Select(task => task.Result);

            var albumEntries = albumChart.Entries
                .Select(async lastFmEntry => new ChartEntry()
                {
                    Week = chartWeek,
                    Type = ChartEntryType.Album,
                    Rank = lastFmEntry.Rank,
                    Album = await GetAlbumOrCreate(lastFmEntry.Artist, lastFmEntry.Title)
                })
                .Select(task => task.Result);

            var artistEntries = artistChart.Entries
                .Select(async lastFmEntry => new ChartEntry()
                {
                    Week = chartWeek,
                    Type = ChartEntryType.Artist,
                    Rank = lastFmEntry.Rank,
                    Artist = await GetArtistOrCreate(lastFmEntry.Name)
                })
                .Select(task => task.Result);

            return trackEntries.Concat(albumEntries).Concat(artistEntries).ToList();
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
                    prevEntry.Track?.Title == entry.Track?.Title
                    && prevEntry.Track?.ArtistName == entry.Track?.ArtistName);

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
                .SelectMany(week => week.ChartEntries)
                .ToList()
                .Find(prevEntry =>
                    prevEntry.Track?.Title == entry.Track?.Title
                    && prevEntry.Track?.ArtistName == entry.Track?.ArtistName);

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

                await _context.Tracks.AddAsync(track);
                await _context.SaveChangesAsync();
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

                await _context.Albums.AddAsync(album);
                await _context.SaveChangesAsync();
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

                await _context.Artists.AddAsync(artist);
                await _context.SaveChangesAsync();
            }

            return artist;
        }

        private DateTime GetChartEndDateForStartDate(DateTime chartDate)
        {
            return chartDate
                    .AddDays(6)
                    .AddHours(23)
                    .AddMinutes(59)
                    .AddSeconds(59);
        }
    }
}