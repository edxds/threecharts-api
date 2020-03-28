namespace ThreeChartsAPI.Models
{
    public class ChartEntry
    {
        public int Id { get; set; }
        public ChartEntryType Type { get; set; }
        public ChartEntryStat Stat { get; set; }
        public string StatText { get; set; } = null!;

        public int? TrackId { get; set; }
        public Track? Track { get; set; }

        public int? AlbumId { get; set; }
        public Album? Album { get; set; }

        public int? ArtistId { get; set; }
        public Artist? Artist { get; set; }

        public int WeekId { get; set; }
        public ChartWeek Week { get; set; } = null!;
    }

    public enum ChartEntryType { Album, Artist, Track }
}