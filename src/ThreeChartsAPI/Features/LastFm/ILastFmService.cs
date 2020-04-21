using System.Threading.Tasks;
using FluentResults;
using ThreeChartsAPI.Features.LastFm.Models;

namespace ThreeChartsAPI.Features.LastFm
{
    public interface ILastFmService
    {
        string GetAuthorizationUrl(string? callback);
        Task<Result<LastFmSession>> CreateLastFmSession(string token);
        Task<Result<LastFmUserInfo>> GetUserInfo(string? userName, string? session);
        Task<Result<LastFmChart<LastFmChartTrack>>> GetWeeklyTrackChart(string user, long from, long to);
        Task<Result<LastFmChart<LastFmChartAlbum>>> GetWeeklyAlbumChart(string user, long from, long to);
        Task<Result<LastFmChart<LastFmChartArtist>>> GetWeeklyArtistChart(string user, long from, long to);
    }
}