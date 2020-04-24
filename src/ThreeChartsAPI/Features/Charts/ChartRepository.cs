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
                .Where(week => week.OwnerId == ownerId);
        }

        public async Task AddWeeksAndSaveChanges(IEnumerable<ChartWeek> weeks)
        {
            await _context.ChartWeeks.AddRangeAsync(weeks);
            await _context.SaveChangesAsync();
        }

        public async Task<Track> GetTrackOrCreate(string artist, string title)
        {
            var track = await _context.Tracks.FirstOrDefaultAsync(a =>
                            a.ArtistName == artist && a.Title == title)
                        ?? new Track { ArtistName = artist, Title = title };

            if (track.Id == 0)
            {
                await _context.Tracks.AddAsync(track);
                await _context.SaveChangesAsync();
            }

            return track;
        }

        public async Task<Album> GetAlbumOrCreate(string artist, string title)
        {
            var album = await _context.Albums.FirstOrDefaultAsync(a =>
                            a.ArtistName == artist && a.Title == title)
                        ?? new Album { ArtistName = artist, Title = title };

            if (album.Id == 0)
            {
                await _context.Albums.AddAsync(album);
                await _context.SaveChangesAsync();
            }

            return album;
        }

        public async Task<Artist> GetArtistOrCreate(string name)
        {
            var artist = await _context.Artists.FirstOrDefaultAsync(a => a.Name == name)
                         ?? new Artist { Name = name };

            if (artist.Id == 0)
            {
                await _context.Artists.AddAsync(artist);
                await _context.SaveChangesAsync();
            }

            return artist;
        }
    }
}