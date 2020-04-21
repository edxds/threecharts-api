using System;
using System.Collections.Generic;
using ThreeChartsAPI.Features.Users.Models;

namespace ThreeChartsAPI.Features.Charts.Models
{
    public class ChartWeek : IEquatable<ChartWeek>
    {
        public int Id { get; set; }
        public int WeekNumber { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public int OwnerId { get; set; }
        public User Owner { get; set; } = null!;

        public List<ChartEntry> ChartEntries { get; set; } = null!;

        public bool Equals(ChartWeek other)
        {
            return Id == other.Id
                && WeekNumber == other.WeekNumber
                && From == other.From
                && To == other.To
                && ChartEntries == other.ChartEntries;
        }
    }
}