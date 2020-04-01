using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FluentResults;
using ThreeChartsAPI.Models.LastFm;

namespace ThreeChartsAPI.Services.LastFm
{
    public class HttpLastFmService : ILastFmService
    {
        private readonly HttpClient _httpClient;
        private readonly ILastFmDeserializer _deserializer;

        private readonly string _apiKey;
        private readonly string _apiSecret;

        public HttpLastFmService(IHttpClientFactory factory, ILastFmDeserializer deserializer, Settings settings)
        {
            _httpClient = factory.CreateClient("lastFm");
            _deserializer = deserializer;
            _apiKey = settings.ApiKey;
            _apiSecret = settings.ApiSecret;
        }

        public async Task<Result<LastFmSession>> CreateLastFmSession(string token)
        {
            var method = "auth.getsession";
            var signature = GenerateSignature(
                ("method", "auth.getsession"),
                ("token", token),
                ("api_key", _apiKey)
            );

            var uri = $"?method={method}&api_key={_apiKey}&token={token}&api_sig={signature}&format=json";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var stream = await response.Content.ReadAsStreamAsync();
                return Results.Ok(await _deserializer.DeserializeSession(stream));
            }

            return await HandleLastFmError(response);
        }

        public async Task<Result<LastFmUserInfo>> GetUserInfo(string? userName, string? session)
        {
            var hasUserName = userName != null;
            var hasSession = session != null;

            if (!hasUserName && !hasSession)
            {
                throw new InvalidOperationException("You must pass either a username or a session key.");
            }

            var uri = hasUserName
                ? $"?method=user.getinfo&api_key={_apiKey}&user={userName}&format=json"
                : $"?method=user.getinfo&api_key={_apiKey}&sk={session}&format=json";

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var stream = await response.Content.ReadAsStreamAsync();
                return Results.Ok(await _deserializer.DeserializeUserInfo(stream));
            }

            return await HandleLastFmError(response);
        }

        public async Task<Result<LastFmChart<LastFmChartAlbum>>> GetWeeklyAlbumChart(string user, long from, long to)
        {
            var uri = $"?method=user.getweeklyalbumchart&api_key={_apiKey}&user={user}&from={from}&to={to}&format=json";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var stream = await response.Content.ReadAsStreamAsync();
                return Results.Ok(await _deserializer.DeserializeAlbumChart(stream));
            }

            return await HandleLastFmError(response);
        }

        public async Task<Result<LastFmChart<LastFmChartArtist>>> GetWeeklyArtistChart(string user, long from, long to)
        {
            var uri = $"?method=user.getweeklyartistchart&api_key={_apiKey}&user={user}&from={from}&to={to}&format=json";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var stream = await response.Content.ReadAsStreamAsync();
                return Results.Ok(await _deserializer.DeserializeArtistChart(stream));
            }

            return await HandleLastFmError(response);
        }

        public async Task<Result<LastFmChart<LastFmChartTrack>>> GetWeeklyTrackChart(string user, long from, long to)
        {
            var uri = $"?method=user.getweeklytrackchart&api_key={_apiKey}&user={user}&from={from}&to={to}&format=json";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var stream = await response.Content.ReadAsStreamAsync();
                return Results.Ok(await _deserializer.DeserializeTrackChart(stream));
            }

            return await HandleLastFmError(response);
        }

        public class Settings
        {
            public string ApiKey { get; }
            public string ApiSecret { get; }

            public Settings(string apiKey, string apiSecret)
            {
                ApiKey = apiKey;
                ApiSecret = apiSecret;
            }
        }

        private async Task<Result> HandleLastFmError(HttpResponseMessage response)
        {
            try
            {
                var error = await _deserializer.DeserializeError(
                    await response.Content.ReadAsStreamAsync()
                );

                return Results.Fail(
                    new LastFmResultError((int)response.StatusCode, error)
                );
            }
            catch (Exception exception)
            {
                // Invalid response with non 2xx response code
                return Results.Fail(
                    new LastFmResultError((int)HttpStatusCode.ServiceUnavailable, null)
                        .CausedBy(exception)
                );
            }
        }

        private string GenerateSignature(params (string, string)[] keyValues)
        {
            using (var md5 = MD5.Create())
            {
                var sortedKeyValues = keyValues.OrderBy(pair => pair.Item1);
                var rawSignature = sortedKeyValues
                    .Select(pair => $"{pair.Item1}{pair.Item2}")
                    .Aggregate((aggregate, str) => aggregate += str);

                var data = md5.ComputeHash(Encoding.UTF8.GetBytes(rawSignature + _apiSecret));
                var builder = new StringBuilder();

                foreach (var @byte in data)
                {
                    builder.Append(@byte.ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}