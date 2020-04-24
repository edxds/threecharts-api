namespace ThreeChartsAPI.Features.Charts.Models
{
    public class Album
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string ArtistName { get; set; } = null!;

        public string? ArtworkUrl { get; set; }
    }
}