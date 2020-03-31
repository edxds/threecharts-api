using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentResults;
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

        public async Task<Result> OnboardUser(User user, DateTime? endDate)
        {
            var weeks = _chartWeekService.GetChartWeeksInDateRange(
                user.RegisteredAt,
                endDate ?? DateTime.Now
            );

            Result? failedResult = null;

            var populatedWeeks = await Task.WhenAll(weeks.Select(async week =>
            {
                if (failedResult != null)
                {
                    // Abort
                    return new ChartWeek();
                }

                week.Owner = user;

                var entriesResult = await _chartWeekService.CreateEntriesForChartWeek(week);
                if (entriesResult.IsFailed)
                {
                    failedResult = entriesResult;
                }

                week.ChartEntries = entriesResult.ValueOrDefault;
                return week;
            }));

            if (failedResult != null)
            {
                return failedResult;
            }

            var entries = populatedWeeks.SelectMany(week => week.ChartEntries).ToList();
            entries.ForEach(entry =>
            {
                var (stat, statText) = _chartWeekService.GetStatsForChartEntry(entry, weeks);
                entry.Stat = stat;
                entry.StatText = statText;
            });

            await _context.ChartWeeks.AddRangeAsync(populatedWeeks);
            await _context.SaveChangesAsync();

            return Results.Ok();
        }
    }
}