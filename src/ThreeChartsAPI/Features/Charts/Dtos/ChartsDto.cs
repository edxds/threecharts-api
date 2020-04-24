using System.Collections.Generic;
using ThreeChartsAPI.Features.Charts.Models;

namespace ThreeChartsAPI.Features.Charts.Dtos
{
    public class EntryDto
    {
        public int Id { get; set; }
        
        public ChartEntryType Type { get; set; }
        
        public int Rank { get; set; }
        public ChartEntryStat Stat { get; set; }
        public string? StatText { get; set; }
        
        public string? Title { get; set; }
        public string Artist { get; set; } = null!;
    }

    public class ChartsDto
    {
        public int OwnerId { get; set; }
        public int WeekId { get; set; }

        public int WeekNumber { get; set; }
        public List<EntryDto> TrackEntries { get; set; } = null!;
        public List<EntryDto> AlbumEntries { get; set; } = null!;
        public List<EntryDto> ArtistEntries { get; set; } = null!;
    }
}