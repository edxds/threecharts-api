namespace ThreeChartsAPI.Features.LastFm.Models
{
    public class LastFmChartTrack
    {
        public string Url { get; set; } = null!;
        public string Artist { get; set; } = null!;
        public string Title { get; set; } = null!;
        public int Rank { get; set; }
        public int PlayCount { get; set; }
    }
}