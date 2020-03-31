using System.Threading.Tasks;
using FluentResults;
using ThreeChartsAPI.Models.LastFm;

namespace ThreeChartsAPI.Services.LastFm
{
    public interface ILastFmService
    {
        Task<Result<LastFmSession>> CreateLastFmSession(string token);
        Task<Result<LastFmUserInfo>> GetUserInfo(string? userName, string? session);
        Task<Result<LastFmChart<LastFmChartTrack>>> GetWeeklyTrackChart(string user, long from, long to);
        Task<Result<LastFmChart<LastFmChartAlbum>>> GetWeeklyAlbumChart(string user, long from, long to);
        Task<Result<LastFmChart<LastFmChartArtist>>> GetWeeklyArtistChart(string user, long from, long to);
    }
}