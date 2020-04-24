using System.Collections.Generic;
using System.Threading;
using FluentResults;
using Moq;
using ThreeChartsAPI.Features.LastFm;
using ThreeChartsAPI.Features.LastFm.Models;

namespace ThreeChartsAPI.Tests
{
    public class FakeLastFmService
    {
        public Mock<ILastFmService> Fake = new Mock<ILastFmService>();
        public ILastFmService Object => Fake.Object;

        public List<LastFmChartTrack> Tracks { get; set; } = new List<LastFmChartTrack>();
        public List<LastFmChartAlbum> Albums { get; set; } = new List<LastFmChartAlbum>();
        public List<LastFmChartArtist> Artists { get; set; } = new List<LastFmChartArtist>();

        public FakeLastFmService()
        {
            SetupFake();
        }

        public void SetupFake()
        {
            Fake
                .Setup(s =>
                    s.GetWeeklyTrackChart(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>(),
                        It.IsAny<CancellationToken?>()))
                .ReturnsAsync(Results.Ok(
                    new LastFmChart<LastFmChartTrack>
                    {
                        Entries = Tracks
                    }
                ))
                .Verifiable();

            Fake
                .Setup(s =>
                    s.GetWeeklyAlbumChart(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>(),
                        It.IsAny<CancellationToken?>()))
                .ReturnsAsync(Results.Ok(
                    new LastFmChart<LastFmChartAlbum> { Entries = Albums })
                )
                .Verifiable();

            Fake
                .Setup(s =>
                    s.GetWeeklyArtistChart(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>(),
                        It.IsAny<CancellationToken?>()))
                .ReturnsAsync(Results.Ok(
                    new LastFmChart<LastFmChartArtist> { Entries = Artists })
                )
                .Verifiable();
        }
    }
}