using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
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
                .ReturnsAsync(new LastFmChart<LastFmChartTrack>()
                {
                    Entries = new List<LastFmChartTrack>()
                    {
                        new LastFmChartTrack() { Title = "Love Again", Artist = "Dua Lipa" }
                    }
                });

            lastFm
                .Setup(s => s.GetWeeklyAlbumChart(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>()))
                .ReturnsAsync(new LastFmChart<LastFmChartAlbum>() { Entries = new List<LastFmChartAlbum>() });

            lastFm
                .Setup(s => s.GetWeeklyArtistChart(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>()))
                .ReturnsAsync(new LastFmChart<LastFmChartArtist>() { Entries = new List<LastFmChartArtist>() });

            var context = ThreeChartsTestContext.BuildInMemoryContext();
            var chartWeekService = new ChartWeekService(context, lastFm.Object);

            var user = new User() { DisplayName = "edxds" };
            var defaultWeek = new ChartWeek()
            {
                Owner = user,
                From = new DateTime(),
                To = new DateTime()
            };

            /* Save two chart entries with the same track */
            for (int i = 0; i < 2; i++)
            {
                await chartWeekService.CreateEntriesForChartWeek(defaultWeek);
            }

            var savedTracks = await context.Tracks.ToListAsync();

            savedTracks.Should().HaveCount(1);
            savedTracks[0].Title.Should().Be("Love Again");
            savedTracks[0].ArtistName.Should().Be("Dua Lipa");
        }
    }
}