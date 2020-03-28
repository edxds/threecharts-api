using System;
using System.Collections.Generic;

namespace ThreeChartsAPI.Models
{
    public class ChartWeek : IEquatable<ChartWeek>
    {
        public int Id { get; set; }
        public int WeekNumber { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }

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