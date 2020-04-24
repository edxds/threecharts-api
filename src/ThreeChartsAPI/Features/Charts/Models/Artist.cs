namespace ThreeChartsAPI.Features.Charts.Models
{
    public class Artist
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public string? ArtworkUrl { get; set; }
    }
}