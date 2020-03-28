using System.Collections.Generic;

namespace ThreeChartsAPI.Models
{
    public class Album
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string ArtistName { get; set; } = null!;

        public List<ChartEntry> Entries { get; set; } = null!;
    }
}