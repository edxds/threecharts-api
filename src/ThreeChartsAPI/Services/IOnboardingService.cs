using System;
using System.Threading.Tasks;
using FluentResults;
using ThreeChartsAPI.Models;

namespace ThreeChartsAPI.Services.Onboarding
{
    public interface IOnboardingService
    {
        Task<Result> OnboardUser(User user, DateTime? endDate);
    }
}