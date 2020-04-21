using System.Collections.Generic;
using ThreeChartsAPI.Features.Charts.Models;

namespace ThreeChartsAPI.Features.Charts.Dtos
{
    public class TrackEntryDto
    {
        public int Id { get; set; }
        public int TrackId { get; set; }

        public int Rank { get; set; }
        public ChartEntryStat Stat { get; set; }
        public string? StatText { get; set; }

        public string Title { get; set; } = null!;
        public string ArtistName { get; set; } = null!;
    }

    public class AlbumEntryDto
    {
        public int Id { get; set; }
        public int AlbumId { get; set; }

        public int Rank { get; set; }
        public ChartEntryStat Stat { get; set; }
        public string? StatText { get; set; }

        public string Title { get; set; } = null!;
        public string ArtistName { get; set; } = null!;
    }

    public class ArtistEntryDto
    {
        public int Id { get; set; }
        public int ArtistId { get; set; }

        public int Rank { get; set; }
        public ChartEntryStat Stat { get; set; }
        public string? StatText { get; set; }

        public string Name { get; set; } = null!;
    }

    public class ChartsDto
    {
        public int OwnerId { get; set; }
        public int WeekId { get; set; }

        public int WeekNumber { get; set; }
        public List<TrackEntryDto> TrackEntries { get; set; } = null!;
        public List<AlbumEntryDto> AlbumEntries { get; set; } = null!;
        public List<ArtistEntryDto> ArtistEntries { get; set; } = null!;
    }
}