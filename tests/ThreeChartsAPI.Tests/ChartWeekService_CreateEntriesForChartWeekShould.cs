using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Moq;
using ThreeChartsAPI.Models;
using ThreeChartsAPI.Models.LastFm;
using ThreeChartsAPI.Services;
using ThreeChartsAPI.Services.LastFm;
using Xunit;

namespace ThreeChartsAPI.Tests
{
    public class ChartWeekService_CreateEntriesForChartWeekShould
    {
        [Fact]
        public async Task CreateEntriesForChartWeek_ShouldSaveTracksWithoutRepetition()
        {
            var lastFm = new Mock<ILastFmService>();
            lastFm
                .Setup(s => s.GetWeeklyTrackChart(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>()))
                .ReturnsAsync(Results.Ok(
                    new LastFmChart<LastFmChartTrack>()
                    {
                        Entries = new List<LastFmChartTrack>()
                        {
                            new LastFmChartTrack() { Title = "Love Again", Artist = "Dua Lipa" }
                        }
                    }
                ));

            lastFm
                .Setup(s => s.GetWeeklyAlbumChart(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>()))
                .ReturnsAsync(Results.Ok(
                    new LastFmChart<LastFmChartAlbum>() { Entries = new List<LastFmChartAlbum>() })
                );

            lastFm
                .Setup(s => s.GetWeeklyArtistChart(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>()))
                .ReturnsAsync(Results.Ok(
                    new LastFmChart<LastFmChartArtist>() { Entries = new List<LastFmChartArtist>() })
                );

            var context = ThreeChartsTestContext.BuildInMemoryContext();
            var chartWeekService = new ChartWeekService(context);

            var user = new User() { UserName = "edxds" };
            var defaultWeek = new ChartWeek()
            {
                Owner = user,
                From = new DateTime(),
                To = new DateTime()
            };

            /* Save two chart entries with the same track */
            for (int i = 0; i < 2; i++)
            {
                var trackChart = await lastFm.Object.GetWeeklyTrackChart("", 0, 0);
                var albumChart = await lastFm.Object.GetWeeklyAlbumChart("", 0, 0);
                var artistChart = await lastFm.Object.GetWeeklyArtistChart("", 0, 0);

                await chartWeekService.CreateEntriesForLastFmCharts(
                    trackChart.Value,
                    albumChart.Value,
                    artistChart.Value,
                    defaultWeek
                );
            }

            var savedTracks = await context.Tracks.ToListAsync();

            savedTracks.Should().HaveCount(1);
            savedTracks[0].Title.Should().Be("Love Again");
            savedTracks[0].ArtistName.Should().Be("Dua Lipa");
        }
    }
}