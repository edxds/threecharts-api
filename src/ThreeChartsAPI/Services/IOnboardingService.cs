using System;
using System.Threading.Tasks;
using ThreeChartsAPI.Models;

namespace ThreeChartsAPI.Services.Onboarding
{
    public interface IOnboardingService
    {
        Task OnboardUser(User user, DateTime? endDate);
    }
}