using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ThreeChartsAPI.Models.LastFm;

namespace ThreeChartsAPI.Services.LastFm
{
    public class LastFmJsonDeserializer : ILastFmDeserializer
    {
        public async Task<LastFmChart<LastFmChartTrack>> DeserializeTrackChart(Stream json)
        {
            using (var jsonDocument = await JsonDocument.ParseAsync(json))
            {
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
        }

        public async Task<LastFmChart<LastFmChartAlbum>> DeserializeAlbumChart(Stream json)
        {
            using (var jsonDocument = await JsonDocument.ParseAsync(json))
            {
                var chartRoot = jsonDocument.RootElement.GetProperty("weeklyalbumchart");
                return new LastFmChart<LastFmChartAlbum>()
                {
                    User = GetUserFromChartRoot(chartRoot),
                    From = GetFromDateFromChartRoot(chartRoot),
                    To = GetToDateFromChartRoot(chartRoot),
                    Entries = chartRoot.GetProperty("album").EnumerateArray().Select(album =>
                        new LastFmChartAlbum()
                        {
                            Url = GetUrlFromLastFmChartItem(album),
                            Artist = GetArtistFromLastFmChartItem(album),
                            Title = GetNameFromLastFmChartItem(album),
                            Rank = GetRankFromLastFmChartItem(album),
                            PlayCount = GetPlayCountFromLastFmChartItem(album)
                        }).ToList()
                };
            }
        }

        public async Task<LastFmChart<LastFmChartArtist>> DeserializeArtistChart(Stream json)
        {
            using (var jsonDocument = await JsonDocument.ParseAsync(json))
            {
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