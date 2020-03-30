namespace ThreeChartsAPI.Models.LastFm
{
    public class LastFmUserInfo
    {
        public string Name { get; set; } = null!;
        public string RealName { get; set; } = null!;
        public string Url { get; set; } = null!;
        public string Image { get; set; } = null!;
        public long RegisterDate { get; set; }
    }
}