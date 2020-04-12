using System.Collections.Generic;

namespace ThreeChartsAPI.Models
{
    public class Artist
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public string? ArtworkUrl { get; set; }

        public List<ChartEntry> Entries { get; set; } = null!;
    }
}