namespace ThreeChartsAPI.Models.LastFm
{
    public class LastFmError
    {
        public int ErrorCode { get; set; }
        public string Message { get; set; } = null!;
    }
}