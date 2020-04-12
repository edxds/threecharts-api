using System.Threading.Tasks;
using ThreeChartsAPI.Models;

namespace ThreeChartsAPI.Services
{
    public interface IArtworkService
    {
        Task<string?> GetArtworkUrlFor(Track track);
        Task<string?> GetArtworkUrlFor(Album album);
        Task<string?> GetArtworkUrlFor(Artist artist);
    }
}