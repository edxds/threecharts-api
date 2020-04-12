using System.Threading.Tasks;
using SpotifyAPI.Web;

namespace ThreeChartsAPI.Services
{
    public interface ISpotifyAPIProvider
    {
        Task<SpotifyWebAPI> GetAPI();
    }
}