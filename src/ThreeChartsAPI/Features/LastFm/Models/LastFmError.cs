namespace ThreeChartsAPI.Features.LastFm.Models
{
    public class LastFmError
    {
        public int ErrorCode { get; set; }
        public string Message { get; set; } = null!;
    }
}