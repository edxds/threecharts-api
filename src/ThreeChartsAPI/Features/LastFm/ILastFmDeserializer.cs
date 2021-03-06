using System.IO;
using System.Threading.Tasks;
using ThreeChartsAPI.Features.LastFm.Models;

namespace ThreeChartsAPI.Features.LastFm
{
    public interface ILastFmDeserializer
    {
        Task<LastFmError> DeserializeError(Stream json);
        Task<LastFmSession> DeserializeSession(Stream json);
        Task<LastFmUserInfo> DeserializeUserInfo(Stream json);
        Task<LastFmChart<LastFmChartTrack>> DeserializeTrackChart(Stream json);
        Task<LastFmChart<LastFmChartAlbum>> DeserializeAlbumChart(Stream json);
        Task<LastFmChart<LastFmChartArtist>> DeserializeArtistChart(Stream json);
    }
}