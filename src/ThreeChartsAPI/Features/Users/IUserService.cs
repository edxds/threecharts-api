using System.Threading.Tasks;
using FluentResults;
using ThreeChartsAPI.Features.LastFm.Models;
using ThreeChartsAPI.Features.Users.Dtos;

namespace ThreeChartsAPI.Features.Users
{
    public interface IUserService
    {
        Task<Result> UpdateUserPreferences(Models.User user, UserPreferencesDto newPreferences);
        Task<Models.User> GetOrCreateUserFromInfo(LastFmUserInfo userInfo);
        Task<Models.User> FindUserFromUserName(string userName);
    }
}