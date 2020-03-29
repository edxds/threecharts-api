using System.Collections.Generic;

namespace ThreeChartsAPI.Models.LastFm
{
    public class LastFmChart<T>
    {
        public string User { get; set; } = null!;
        public long From { get; set; }
        public long To { get; set; }
        public List<T> Entries { get; set; } = null!;
    }
}