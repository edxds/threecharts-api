using System;
using System.Threading.Tasks;
using FluentResults;
using ThreeChartsAPI.Models;

namespace ThreeChartsAPI.Services.Onboarding
{
    public interface IOnboardingService
    {
        Task<Result> SyncWeeks(
            User user,
            int startWeekNumber,
            DateTime startDate,
            DateTime? endDate,
            TimeZoneInfo timeZone);
    }
}