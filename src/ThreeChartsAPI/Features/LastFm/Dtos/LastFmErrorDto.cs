namespace ThreeChartsAPI.Features.LastFm.Dtos
{
    public class LastFmErrorDto
    {
        public int ErrorCode { get; set; }
        public string Message { get; set; } = null!;
    }
}