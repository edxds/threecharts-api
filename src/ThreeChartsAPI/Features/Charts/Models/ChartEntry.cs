using System;
using System.Diagnostics.CodeAnalysis;

namespace ThreeChartsAPI.Features.Charts.Models
{
    public class ChartEntry : IEquatable<ChartEntry>
    {
        public int Id { get; set; }

        public int Rank { get; set; }

        public ChartEntryType Type { get; set; }
        public ChartEntryStat Stat { get; set; }
        public string? StatText { get; set; }

        public int? TrackId { get; set; }
        public Track? Track { get; set; }

        public int? AlbumId { get; set; }
        public Album? Album { get; set; }

        public int? ArtistId { get; set; }
        public Artist? Artist { get; set; }

        public int WeekId { get; set; }
        public ChartWeek Week { get; set; } = null!;

        public bool Equals(ChartEntry other)
        {
            return Id == other.Id
                && Type == other.Type
                && Stat == other.Stat
                && StatText == other.StatText
                && TrackId == other.TrackId
                && AlbumId == other.AlbumId
                && ArtistId == other.ArtistId
                && WeekId == other.WeekId;
        }
    }

    public enum ChartEntryType { Album, Artist, Track }
}