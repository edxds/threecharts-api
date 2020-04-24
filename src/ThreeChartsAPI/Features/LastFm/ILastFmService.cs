using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using ThreeChartsAPI.Features.LastFm.Models;

namespace ThreeChartsAPI.Features.LastFm
{
    public interface ILastFmService
    {
        string GetAuthorizationUrl(string? callback);

        Task<Result<LastFmSession>> CreateLastFmSession(
            string token,
            CancellationToken? cancellationToken = null);

        Task<Result<LastFmUserInfo>> GetUserInfo(
            string? userName,
            string? session,
            CancellationToken? cancellationToken = null);

        Task<Result<LastFmChart<LastFmChartTrack>>> GetWeeklyTrackChart(
            string user,
            long from,
            long to,
            CancellationToken? cancellationToken = null);

        Task<Result<LastFmChart<LastFmChartAlbum>>> GetWeeklyAlbumChart(
            string user,
            long from,
            long to,
            CancellationToken? cancellationToken = null);

        Task<Result<LastFmChart<LastFmChartArtist>>> GetWeeklyArtistChart(
            string user,
            long from,
            long to,
            CancellationToken? cancellationToken = null);
    }
}