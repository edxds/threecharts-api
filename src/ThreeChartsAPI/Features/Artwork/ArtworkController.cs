using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThreeChartsAPI.Features.Charts;

namespace ThreeChartsAPI.Features.Artwork
{
    [Authorize]
    [ApiController]
    [Route("api/artwork")]
    public class ArtworkController : ControllerBase
    {
        private readonly ChartRepository _repo;
        private readonly IArtworkService _artworkService;

        public ArtworkController(ChartRepository repo, IArtworkService artworkService)
        {
            _repo = repo;
            _artworkService = artworkService;
        }
        
        [HttpGet("track/{artist}/{title}")]
        public async Task<ActionResult> GetTrackArtwork(string artist, string title)
        {
            var track = await _repo.GetTrackOrCreate(artist, title);
            if (track == null)
            {
                return NotFound();
            }

            var artworkUrl = await _artworkService.GetArtworkUrlFor(track);
            if (artworkUrl == null)
            {
                return NotFound();
            }
            
            return Redirect(artworkUrl);
        }

        [HttpGet("album/{artist}/{title}")]
        public async Task<ActionResult> GetAlbumArtwork(string artist, string title)
        {
            var album = await _repo.GetAlbumOrCreate(artist, title);
            if (album == null)
            {
                return NotFound();
            }

            var artworkUrl = await _artworkService.GetArtworkUrlFor(album);
            if (artworkUrl == null)
            {
                return NotFound();
            }

            return Redirect(artworkUrl);
        }
        
        [HttpGet("artist/{name}")]
        public async Task<ActionResult> GetArtistArtwork(string name)
        {
            var artist = await _repo.GetArtistOrCreate(name);
            if (artist == null)
            {
                return NotFound();
            }

            var artworkUrl = await _artworkService.GetArtworkUrlFor(artist);
            if (artworkUrl == null)
            {
                return NotFound();
            }

            return Redirect(artworkUrl);
        }
    }
}