namespace ThreeChartsAPI.Features.LastFm.Models
{
    public class LastFmChartArtist
    {
        public string Url { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int Rank { get; set; }
        public int PlayCount { get; set; }
    }
}