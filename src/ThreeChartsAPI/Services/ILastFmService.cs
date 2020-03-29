using System.Threading.Tasks;
using ThreeChartsAPI.Models.LastFm;

namespace ThreeChartsAPI.Services.LastFm
{
    public interface ILastFmService
    {
        Task<LastFmChart<LastFmChartTrack>> GetWeeklyTrackChart(string user, long from, long to);
        Task<LastFmChart<LastFmChartAlbum>> GetWeeklyAlbumChart(string user, long from, long to);
        Task<LastFmChart<LastFmChartArtist>> GetWeeklyArtistChart(string user, long from, long to);
    }
}