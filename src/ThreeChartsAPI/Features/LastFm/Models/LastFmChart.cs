using System.Collections.Generic;

namespace ThreeChartsAPI.Features.LastFm.Models
{
    public class LastFmChart<T>
    {
        public string User { get; set; } = null!;
        public long From { get; set; }
        public long To { get; set; }
        public List<T> Entries { get; set; } = null!;
    }
}