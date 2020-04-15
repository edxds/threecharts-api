using System.Threading.Tasks;
using FluentResults;
using ThreeChartsAPI.Models;
using ThreeChartsAPI.Models.LastFm;

namespace ThreeChartsAPI.Services
{
    public interface IUserService
    {
        Task<Result> UpdateUserPreferences(User user, UserPreferencesDto newPreferences);
        Task<User> GetOrCreateUserFromInfo(LastFmUserInfo userInfo);
        Task<User> FindUserFromUserName(string userName);
    }
}