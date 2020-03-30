using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ThreeChartsAPI.Models;

namespace ThreeChartsAPI.Services.Onboarding
{
    public class OnboardingService : IOnboardingService
    {
        private readonly ThreeChartsContext _context;
        private readonly IChartWeekService _chartWeekService;

        public OnboardingService(ThreeChartsContext context, IChartWeekService chartWeekService)
        {
            _context = context;
            _chartWeekService = chartWeekService;
        }

        public async Task OnboardUser(User user, DateTime? endDate)
        {
            var weeks = _chartWeekService.GetChartWeeksInDateRange(
                user.RegisteredAt,
                endDate ?? DateTime.Now
            );

            var populatedWeeks = await Task.WhenAll(weeks.Select(async week =>
            {
                week.Owner = user;
                week.ChartEntries = await _chartWeekService.CreateEntriesForChartWeek(week);
                return week;
            }));

            var entries = populatedWeeks.SelectMany(week => week.ChartEntries).ToList();
            entries.ForEach(entry =>
            {
                var (stat, statText) = _chartWeekService.GetStatsForChartEntry(entry, weeks);
                entry.Stat = stat;
                entry.StatText = statText;
            });

            await _context.ChartWeeks.AddRangeAsync(populatedWeeks);
            await _context.SaveChangesAsync();
        }
    }
}