using System.Threading.Tasks;
using ThreeChartsAPI.Models;
using ThreeChartsAPI.Models.LastFm;

namespace ThreeChartsAPI.Services
{
    public interface IUserService
    {
        Task<User> GetOrCreateUserFromInfo(LastFmUserInfo userInfo);
    }
}