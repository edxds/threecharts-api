using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using ThreeChartsAPI.Features.Charts.Models;
using ThreeChartsAPI.Features.Spotify;

namespace ThreeChartsAPI.Features.Artwork
{
    public class SpotifyArtworkService : IArtworkService
    {
        private readonly ThreeChartsContext _context;
        private readonly ILogger<SpotifyArtworkService> _logger;
        private readonly ISpotifyAPIProvider _spotifyApiProvider;

        public SpotifyArtworkService(
            ISpotifyAPIProvider apiProvider,
            ThreeChartsContext context,
            ILogger<SpotifyArtworkService> logger)
        {
            _context = context;
            _logger = logger;
            _spotifyApiProvider = apiProvider;
        }

        public async Task<string?> GetArtworkUrlFor(Track track)
        {
            if (track.ArtworkUrl != null) return track.ArtworkUrl;

            var sw = StartStopwatch();
            var spotify = await _spotifyApiProvider.GetAPI();

            var query = $"{track.ArtistName} ${track.Title}";
            var search = await spotify.SearchItemsEscapedAsync(query, SearchType.Track, 1);
            if (CheckForErrorsAndLog(search)) return null;

            var searchedTrack = search.Tracks.Items.First();
            var albumImages = searchedTrack.Album.Images;
            var desiredArtwork = albumImages.FirstOrDefault();

            if (desiredArtwork == null)
            {
                return null;
            }

            track.ArtworkUrl = desiredArtwork.Url;
            await _context.SaveChangesAsync();

            StopStopwatchAndLogTime(sw);
            return track.ArtworkUrl;
        }

        public async Task<string?> GetArtworkUrlFor(Album album)
        {
            if (album.ArtworkUrl != null) return album.ArtworkUrl;

            var sw = StartStopwatch();
            var spotify = await _spotifyApiProvider.GetAPI();

            var query = $"${album.ArtistName} ${album.Title}";
            var search = await spotify.SearchItemsEscapedAsync(query, SearchType.Album, 1);
            if (CheckForErrorsAndLog(search)) return null;

            var searchedAlbum = search.Albums.Items.First();
            var desiredArtwork = searchedAlbum.Images.FirstOrDefault();

            if (desiredArtwork == null)
            {
                return null;
            }

            album.ArtworkUrl = desiredArtwork.Url;
            await _context.SaveChangesAsync();

            StopStopwatchAndLogTime(sw);
            return album.ArtworkUrl;
        }

        public async Task<string?> GetArtworkUrlFor(Artist artist)
        {
            if (artist.ArtworkUrl != null) return artist.ArtworkUrl;

            var sw = StartStopwatch();
            var spotify = await _spotifyApiProvider.GetAPI();

            var query = artist.Name;
            var search = await spotify.SearchItemsEscapedAsync(query, SearchType.Artist, 1);
            if (CheckForErrorsAndLog(search)) return null;

            var searchedArtist = search.Artists.Items.First();
            var desiredArtwork = searchedArtist.Images.FirstOrDefault();

            if (desiredArtwork == null)
            {
                return null;
            }

            artist.ArtworkUrl = desiredArtwork.Url;
            await _context.SaveChangesAsync();

            StopStopwatchAndLogTime(sw);
            return artist.ArtworkUrl;
        }

        private Stopwatch StartStopwatch()
        {
            var watch = new Stopwatch();
            watch.Start();

            return watch;
        }

        private void StopStopwatchAndLogTime(Stopwatch stopwatch)
        {
            stopwatch.Stop();
            var ts = stopwatch.Elapsed;
            
            _logger.LogInformation($"Finished Spotify call after {ts.Milliseconds}ms");
        }
        
        private bool CheckForErrorsAndLog(SearchItem item)
        {
            var hasError = item.HasError();
            if (hasError)
            {
                _logger.LogWarning(
                    $"Spotify call failed with response code {item.StatusCode()} and error {item.Error.Message}");
            }

            return hasError;
        }
    }
}