using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThreeChartsAPI.Models;
using ThreeChartsAPI.Services;

namespace ThreeChartsAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/artwork")]
    public class ArtworkController : ControllerBase
    {
        private readonly ThreeChartsContext _context;
        private readonly IArtworkService _artworkService;

        public ArtworkController(ThreeChartsContext context, IArtworkService artworkService)
        {
            _context = context;
            _artworkService = artworkService;
        }
        
        [HttpGet("track/{trackId}")]
        public async Task<ActionResult> GetTrackArtwork(int trackId)
        {
            var track = await _context.Tracks.FindAsync(trackId);
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

        [HttpGet("album/{albumId}")]
        public async Task<ActionResult> GetAlbumArtwork(int albumId)
        {
            var album = await _context.Albums.FindAsync(albumId);
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
        
        [HttpGet("artist/{artistId}")]
        public async Task<ActionResult> GetArtistArtwork(int artistId)
        {
            var artist = await _context.Artists.FindAsync(artistId);
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