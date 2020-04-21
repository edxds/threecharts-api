using System.Threading.Tasks;
using SpotifyAPI.Web;

namespace ThreeChartsAPI.Features.Spotify
{
    public interface ISpotifyAPIProvider
    {
        Task<SpotifyWebAPI> GetAPI();
    }
}