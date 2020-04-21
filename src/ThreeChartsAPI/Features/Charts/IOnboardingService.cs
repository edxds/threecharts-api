using System;
using System.Threading.Tasks;
using FluentResults;
using ThreeChartsAPI.Features.Users.Models;

namespace ThreeChartsAPI.Features.Charts
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