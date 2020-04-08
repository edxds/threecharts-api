using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThreeChartsAPI.Models;
using ThreeChartsAPI.Models.LastFm;

namespace ThreeChartsAPI.Services
{
    public class ChartWeekService : IChartWeekService
    {
        private readonly ThreeChartsContext _context;

        public ChartWeekService(ThreeChartsContext context)
        {
            _context = context;
        }

        public List<ChartWeek> GetChartWeeksInDateRange(
            int startWeekNumber,
            DateTime startDate,
            DateTime endDate,
            TimeZoneInfo timeZone)
        {
            var chartWeekList = new List<ChartWeek>();

            var startDateWithoutTimeUnspecified = DateTime.SpecifyKind(new DateTime(
                startDate.Year,
                startDate.Month,
                startDate.Day), DateTimeKind.Unspecified);

            var startDateWithoutTimeUtc = TimeZoneInfo.ConvertTimeToUtc(
                startDateWithoutTimeUnspecified,
                timeZone);

            var daysUntilFriday = DayOfWeek.Friday - startDateWithoutTimeUtc.DayOfWeek;
            var firstChartStartDate = startDateWithoutTimeUtc.AddDays(daysUntilFriday);
            var firstChartEndDate = GetChartEndDateForStartDate(firstChartStartDate);

            if (firstChartEndDate > endDate)
            {
                return chartWeekList;
            }

            var currentChartStartDate = firstChartStartDate;
            var currentChartEndDate = firstChartEndDate;
            var currentWeekNumber = startWeekNumber;

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

        public async Task<List<ChartWeek>> GetOutdatedWeeks(
            int ownerId,
            DateTime defaultStartDate,
            DateTime endDate,
            TimeZoneInfo timeZone)
        {
            var lastWeek = await _context.ChartWeeks.Where(week => week.OwnerId == ownerId)
                .OrderByDescending(week => week.WeekNumber)
                .FirstOrDefaultAsync();

            return GetChartWeeksInDateRange(
                lastWeek?.WeekNumber + 1 ?? 1,
                lastWeek?.To.AddSeconds(1) ?? defaultStartDate,
                endDate,
                timeZone);
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