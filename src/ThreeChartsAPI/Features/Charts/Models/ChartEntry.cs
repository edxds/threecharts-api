namespace ThreeChartsAPI.Features.Charts.Models
{
    public class ChartEntry
    {
        public int Id { get; set; }

        public int Rank { get; set; }

        public ChartEntryType Type { get; set; }
        public ChartEntryStat Stat { get; set; }
        public string? StatText { get; set; }

        public string? Title { get; set; }
        public string Artist { get; set; } = null!;

        public int WeekId { get; set; }
        public ChartWeek Week { get; set; } = null!;
    }

    public enum ChartEntryType { Album, Artist, Track }
}