using System.Threading.Tasks;
using ThreeChartsAPI.Features.Charts.Models;

namespace ThreeChartsAPI.Features.Artwork
{
    public interface IArtworkService
    {
        Task<string?> GetArtworkUrlFor(Track track);
        Task<string?> GetArtworkUrlFor(Album album);
        Task<string?> GetArtworkUrlFor(Artist artist);
    }
}