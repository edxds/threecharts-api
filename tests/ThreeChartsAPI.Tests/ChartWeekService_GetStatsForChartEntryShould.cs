using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Moq;
using ThreeChartsAPI.Models;
using ThreeChartsAPI.Models.LastFm;
using ThreeChartsAPI.Services;
using ThreeChartsAPI.Services.LastFm;
using Xunit;

namespace ThreeChartsAPI.Tests
{
    public class ChartWeekService_GetStatsForChartEntryShould
    {
        [Fact]
        public async Task GetStatsForChart_ReturnsCorrectStats()
        {
            var lastFmMock = new Mock<ILastFmService>();
            lastFmMock
                .SetupSequence(s => s.GetWeeklyTrackChart(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>()))
                .ReturnsAsync(Results.Ok(
                    new LastFmChart<LastFmChartTrack>()
                    {
                        Entries = new List<LastFmChartTrack>()
                        {
                            new LastFmChartTrack() { Title = "Cool", Artist = "Dua Lipa", Rank = 1 },
                            new LastFmChartTrack() { Title = "Pretty Please", Artist = "Dua Lipa", Rank = 2 },
                            new LastFmChartTrack() { Title = "Hallucinate", Artist = "Dua Lipa", Rank = 3 },
                            new LastFmChartTrack() { Title = "WANNABE", Artist = "ITZY", Rank = 4 },
                        }
                    })
                )
                .ReturnsAsync(Results.Ok(
                    new LastFmChart<LastFmChartTrack>()
                    {
                        Entries = new List<LastFmChartTrack>()
                        {
                            new LastFmChartTrack() { Title = "WANNABE", Artist = "ITZY", Rank = 1 },
                            new LastFmChartTrack() { Title = "Pretty Please", Artist = "Dua Lipa", Rank = 2 },
                            new LastFmChartTrack() { Title = "Cool", Artist = "Dua Lipa", Rank = 3 },
                        }
                    })
                )
                .ReturnsAsync(Results.Ok(
                    new LastFmChart<LastFmChartTrack>()
                    {
                        Entries = new List<LastFmChartTrack>()
                        {
                            new LastFmChartTrack() { Title = "WANNABE", Artist = "ITZY", Rank = 1 },
                            new LastFmChartTrack() { Title = "Pretty Please", Artist = "Dua Lipa", Rank = 2 },
                            new LastFmChartTrack() { Title = "Hallucinate", Artist = "Dua Lipa", Rank = 3 },
                            new LastFmChartTrack() { Title = "Cool", Artist = "Dua Lipa", Rank = 4 },
                        }
                    })
                );

            lastFmMock
                .Setup(s => s.GetWeeklyAlbumChart(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>()))
                .ReturnsAsync(Results.Ok(
                    new LastFmChart<LastFmChartAlbum>()
                    {
                        Entries = new List<LastFmChartAlbum>()
                    })
                );

            lastFmMock
                .Setup(s => s.GetWeeklyArtistChart(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>()))
                .ReturnsAsync(Results.Ok(
                    new LastFmChart<LastFmChartArtist>()
                    {
                        Entries = new List<LastFmChartArtist>()
                    })
                );

            var context = ThreeChartsTestContext.BuildInMemoryContext();
            var service = new ChartWeekService(context);

            var weeks = new List<ChartWeek>();
            for (int i = 0; i < 3; i++)
            {
                var week = new ChartWeek();
                week.Owner = new User() { UserName = "edxds" };
                week.WeekNumber = i + 1;

                var trackChart = await lastFmMock.Object.GetWeeklyTrackChart("", 0, 0);
                var albumChart = await lastFmMock.Object.GetWeeklyAlbumChart("", 0, 0);
                var artistChart = await lastFmMock.Object.GetWeeklyArtistChart("", 0, 0);

                week.ChartEntries = await service.CreateEntriesForLastFmCharts(
                    trackChart.Value,
                    albumChart.Value,
                    artistChart.Value,
                    week
                );

                weeks.Add(week);
            }

            var firstWeekResults = weeks[0].ChartEntries.Select(entry
                => service.GetStatsForChartEntry(entry, weeks)).ToList();

            var secondWeekResults = weeks[1].ChartEntries.Select(entry
                => service.GetStatsForChartEntry(entry, weeks)).ToList();

            var thirdWeekResults = weeks[2].ChartEntries.Select(entry
                => service.GetStatsForChartEntry(entry, weeks)).ToList();

            // All stats on the first week should be .New
            firstWeekResults.ForEach(r => r.stat.Should().Be(ChartEntryStat.New));

            secondWeekResults.Should().BeEquivalentTo(
                new List<(ChartEntryStat stat, string statText)>()
                {
                    (ChartEntryStat.Increase, "+3"),
                    (ChartEntryStat.NoDiff, "="),
                    (ChartEntryStat.Decrease, "-2"),
                }
            );

            thirdWeekResults.Should().BeEquivalentTo(
                new List<(ChartEntryStat stat, string statText)>()
                {
                    (ChartEntryStat.NoDiff, "="),
                    (ChartEntryStat.NoDiff, "="),
                    (ChartEntryStat.Reentry, null),
                    (ChartEntryStat.Decrease, "-1"),
                }
            );
        }
    }
}