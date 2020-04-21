using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using ThreeChartsAPI.Features.Spotify.Models;

namespace ThreeChartsAPI.Features.Spotify
{
    public class DefaultSpotifyAPIProvider : ISpotifyAPIProvider
    {
        private readonly ThreeChartsContext _context;
        private readonly ILogger<DefaultSpotifyAPIProvider> _logger;
        private readonly string _clientId;
        private readonly string _clientSecret;
        
        public class Settings
        {
            public string ClientId { get; }
            public string ClientSecret { get; }

            public Settings(string clientId, string clientSecret)
            {
                ClientId = clientId;
                ClientSecret = clientSecret;
            }
        }

        public DefaultSpotifyAPIProvider(
            Settings settings,
            ThreeChartsContext context,
            ILogger<DefaultSpotifyAPIProvider> logger)
        {
            _clientId = settings.ClientId;
            _clientSecret = settings.ClientSecret;
            _context = context;
            _logger = logger;
        }
        
        public async Task<SpotifyWebAPI> GetAPI()
        {
            var token = await GetToken();
            return new SpotifyWebAPI()
            {
                TokenType = token.Type,
                AccessToken = token.Token,
            };
        }

        private async Task<SpotifyApiToken> GetToken(int retryCount = 0, Exception? exception = null)
        {
            if (retryCount > 1)
            {
                throw exception ?? new InvalidOperationException();
            }
            
            var existingToken = await _context.SpotifyApiTokens.FirstOrDefaultAsync();
            if (existingToken != null && existingToken.ExpiresAt >= DateTime.UtcNow) 
                return existingToken;
            
            var credentials = new CredentialsAuth(_clientId, _clientSecret);
            var newSpotifyToken = await credentials.GetToken();
            var newExpirationDate = DateTime.UtcNow.AddSeconds(newSpotifyToken.ExpiresIn - 5);
                
            var newToken = new SpotifyApiToken()
            {
                ExpiresAt = newExpirationDate,
                Token = newSpotifyToken.AccessToken,
                Type =  newSpotifyToken.TokenType,
            };

            try
            {
                if (existingToken != null) _context.SpotifyApiTokens.Remove(existingToken);
                await _context.AddAsync(newToken);
                await _context.SaveChangesAsync();
            }
            catch (Exception newException)
            {
                return await GetToken(retryCount + 1, newException);
            }

            _logger.LogInformation("Created new Spotify access token");
            return newToken;
        }
    }
}