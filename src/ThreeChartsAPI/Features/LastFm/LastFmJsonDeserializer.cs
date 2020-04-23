using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ThreeChartsAPI.Features.LastFm.Models;

namespace ThreeChartsAPI.Features.LastFm
{
    public class LastFmJsonDeserializer : ILastFmDeserializer
    {
        private readonly ILogger<LastFmJsonDeserializer> _logger;

        public LastFmJsonDeserializer(ILogger<LastFmJsonDeserializer> logger)
        {
            _logger = logger;
        }
        
        public async Task<LastFmError> DeserializeError(Stream json)
        {
            try
            {
                using var document = await JsonDocument.ParseAsync(json);
                return new LastFmError()
                {
                    ErrorCode = document.RootElement.GetProperty("error").GetInt32(),
                    Message = document.RootElement.GetProperty("message").GetString(),
                };
            }
            catch (Exception)
            {
                _logger.LogError($"Offending JSON: {json}");
                throw;
            }
        }

        public async Task<LastFmSession> DeserializeSession(Stream json)
        {
            try
            {
                using var document = await JsonDocument.ParseAsync(json);
                var session = document.RootElement.GetProperty("session");

                return new LastFmSession()
                {
                    LastFmUser = session.GetProperty("name").GetString(),
                    Key = session.GetProperty("key").GetString(),
                };
            }
            catch (Exception)
            {
                _logger.LogError($"Offending JSON: {json}");
                throw;
            }
        }

        public async Task<LastFmUserInfo> DeserializeUserInfo(Stream json)
        {
            try
            {
                using var document = await JsonDocument.ParseAsync(json);
                var user = document.RootElement.GetProperty("user");
                var images = user.GetProperty("image").EnumerateArray();
                var registerDate =
                    user.GetProperty("registered").GetProperty("unixtime").GetString();

                return new LastFmUserInfo()
                {
                    Name = user.GetProperty("name").GetString(),
                    Url = user.GetProperty("url").GetString(),
                    Image = images.Last().GetProperty("#text").GetString(),
                    RegisterDate = Convert.ToInt64(registerDate),
                    RealName = user.GetProperty("realname").GetString(),
                };
            }
            catch (Exception)
            {
                _logger.LogError($"Offending JSON: {json}");
                throw;
            }
        }

        public async Task<LastFmChart<LastFmChartTrack>> DeserializeTrackChart(Stream json)
        {
            try
            {
                using var jsonDocument = await JsonDocument.ParseAsync(json);
                var chartRoot = jsonDocument.RootElement.GetProperty("weeklytrackchart");
                return new LastFmChart<LastFmChartTrack>()
                {
                    User = GetUserFromChartRoot(chartRoot),
                    From = GetFromDateFromChartRoot(chartRoot),
                    To = GetToDateFromChartRoot(chartRoot),
                    Entries = chartRoot.GetProperty("track").EnumerateArray().Select(track =>
                        new LastFmChartTrack()
                        {
                            Url = GetUrlFromLastFmChartItem(track),
                            Artist = GetArtistFromLastFmChartItem(track),
                            Title = GetNameFromLastFmChartItem(track),
                            Rank = GetRankFromLastFmChartItem(track),
                            PlayCount = GetPlayCountFromLastFmChartItem(track)
                        }).ToList()
                };
            }
            catch (Exception)
            {
                _logger.LogError($"Offending JSON: {json}");
                throw;
            }
        }

        public async Task<LastFmChart<LastFmChartAlbum>> DeserializeAlbumChart(Stream json)
        {
            try
            {
                using var jsonDocument = await JsonDocument.ParseAsync(json);
                var chartRoot = jsonDocument.RootElement.GetProperty("weeklyalbumchart");
                return new LastFmChart<LastFmChartAlbum>
                {
                    User = GetUserFromChartRoot(chartRoot),
                    From = GetFromDateFromChartRoot(chartRoot),
                    To = GetToDateFromChartRoot(chartRoot),
                    Entries = chartRoot.GetProperty("album").EnumerateArray().Select(album =>
                        new LastFmChartAlbum
                        {
                            Url = GetUrlFromLastFmChartItem(album),
                            Artist = GetArtistFromLastFmChartItem(album),
                            Title = GetNameFromLastFmChartItem(album),
                            Rank = GetRankFromLastFmChartItem(album),
                            PlayCount = GetPlayCountFromLastFmChartItem(album)
                        }).ToList()
                };
            }
            catch (Exception)
            {
                _logger.LogError($"Offending JSON: {json}");
                throw;
            }
        }

        public async Task<LastFmChart<LastFmChartArtist>> DeserializeArtistChart(Stream json)
        {
            try
            {
                using var jsonDocument = await JsonDocument.ParseAsync(json);
                var chartRoot = jsonDocument.RootElement.GetProperty("weeklyartistchart");
                return new LastFmChart<LastFmChartArtist>()
                {
                    User = GetUserFromChartRoot(chartRoot),
                    From = GetFromDateFromChartRoot(chartRoot),
                    To = GetToDateFromChartRoot(chartRoot),
                    Entries = chartRoot.GetProperty("artist").EnumerateArray().Select(artist =>
                        new LastFmChartArtist()
                        {
                            Url = GetUrlFromLastFmChartItem(artist),
                            Name = GetNameFromLastFmChartItem(artist),
                            Rank = GetRankFromLastFmChartItem(artist),
                            PlayCount = GetPlayCountFromLastFmChartItem(artist)
                        }).ToList()
                };
            }
            catch (Exception)
            {
                _logger.LogError($"Offending JSON: {json}");
                throw;
            }
        }

        private string GetUserFromChartRoot(JsonElement element)
            => element.GetProperty("@attr").GetProperty("user").GetString();

        private long GetFromDateFromChartRoot(JsonElement element)
            => Convert.ToInt64(element.GetProperty("@attr").GetProperty("from").GetString());

        private long GetToDateFromChartRoot(JsonElement element)
            => Convert.ToInt64(element.GetProperty("@attr").GetProperty("to").GetString());

        private string GetUrlFromLastFmChartItem(JsonElement element)
            => element.GetProperty("url").GetString();

        private string GetArtistFromLastFmChartItem(JsonElement element)
            => element.GetProperty("artist").GetProperty("#text").GetString();

        private string GetNameFromLastFmChartItem(JsonElement element)
            => element.GetProperty("name").GetString();

        private int GetRankFromLastFmChartItem(JsonElement element)
            => Convert.ToInt32(element.GetProperty("@attr").GetProperty("rank").GetString());

        private int GetPlayCountFromLastFmChartItem(JsonElement element)
            => Convert.ToInt32(element.GetProperty("playcount").GetString());
    }
}