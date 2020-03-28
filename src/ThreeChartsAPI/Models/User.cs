using System;
using System.Collections.Generic;

namespace ThreeChartsAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public int LastfmId { get; set; }

        public string DisplayName { get; set; } = null!;
        public string? RealName { get; set; }
        public string? LastFmUrl { get; set; }
        public string? ProfilePicture { get; set; }

        public DateTime RegisteredAt { get; set; }

        public List<ChartWeek> ChartWeeks { get; set; } = null!;
    }
}