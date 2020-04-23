using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThreeChartsAPI.Features.Charts.Models;

namespace ThreeChartsAPI.Features.Charts
{
    public class ChartRepository
    {
        private readonly ThreeChartsContext _context;

        public ChartRepository(ThreeChartsContext context)
        {
            _context = context;
        }
        
        public IQueryable<ChartWeek> QueryWeeksOf(int ownerId)
        {
            return _context.ChartWeeks.Where(week => week.OwnerId == ownerId);
        }
        
        public IQueryable<ChartWeek> QueryWeeksWithRelationsOf(int ownerId)
        {
            return _context.ChartWeeks
                .Include(week => week.ChartEntries)
                    .ThenInclude(entry => entry.Artist)
                .Include(week => week.ChartEntries)
                    .ThenInclude(entry => entry.Album)
                .Include(week => week.ChartEntries)
                    .ThenInclude(entry => entry.Track)
                .Where(week => week.OwnerId == ownerId);
        }

        public async Task AddWeeksAndSaveChanges(IEnumerable<ChartWeek> weeks)
        {
            await _context.ChartWeeks.AddRangeAsync(weeks);
            await _context.SaveChangesAsync();
        }

        public async Task AddMusicalEntitiesFromWeekAndSaveChanges(ChartWeek week)
        {
            var artists = week.ChartEntries.Select(e => e.Artist).Where(a => a != null && a.Id == 0);
            var albums = week.ChartEntries.Select(e => e.Album).Where(a => a != null && a.Id == 0);
            var tracks = week.ChartEntries.Select(e => e.Track).Where(t => t != null && t.Id == 0);

            var entities = artists.Select(o => o as object).Concat(albums).Concat(tracks);

            await _context.AddRangeAsync(entities.Where(e => e != null));
            await _context.SaveChangesAsync();
        }

        public async Task<Track> GetTrackOrCreate(string artist, string title) =>
            await _context.Tracks.FirstOrDefaultAsync(a =>
                a.ArtistName == artist && a.Title == title)
            ?? new Track { ArtistName = artist, Title = title };

        public async Task<Album> GetAlbumOrCreate(string artist, string title) =>
            await _context.Albums.FirstOrDefaultAsync(a =>
                a.ArtistName == artist && a.Title == title)
            ?? new Album { ArtistName = artist, Title = title };

        public async Task<Artist> GetArtistOrCreate(string name) =>
            await _context.Artists.FirstOrDefaultAsync(a => a.Name == name)
            ?? new Artist { Name = name };
    }
}