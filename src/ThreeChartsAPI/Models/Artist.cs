using System.Collections.Generic;

namespace ThreeChartsAPI.Models
{
    public class Artist
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public List<ChartEntry> Entries { get; set; } = null!;
    }
}