using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using ThreeChartsAPI.Models;
using ThreeChartsAPI.Models.LastFm;
using ThreeChartsAPI.Services;
using ThreeChartsAPI.Services.LastFm;

namespace ThreeChartsAPI.Tests
{
    public class ChartWeekService_GetStatsForChartEntryShould
    {
        private readonly ChartWeekService _service;

        public ChartWeekService_GetStatsForChartEntryShould()
        {
            var anyString = It.IsAny<string>();
            var anyLong = It.IsAny<long>();

            var lastFmMock = new Mock<ILastFmService>();
            lastFmMock
                .SetupSequence(s => s.GetWeeklyTrackChart(anyString, anyLong, anyLong))
                .ReturnsAsync(new LastFmChart<LastFmChartTrack>()
                {
                    Entries = new List<LastFmChartTrack>()
                    {
                        new LastFmChartTrack() { Title = "Cool", Artist = "Dua Lipa", Rank = 1 },
                        new LastFmChartTrack() { Title = "Pretty Please", Artist = "Dua Lipa", Rank = 2 },
                        new LastFmChartTrack() { Title = "Hallucinate", Artist = "Dua Lipa", Rank = 3 },
                        new LastFmChartTrack() { Title = "WANNABE", Artist = "ITZY", Rank = 4 },
                    }
                })
                .ReturnsAsync(new LastFmChart<LastFmChartTrack>()
                {
                    Entries = new List<LastFmChartTrack>()
                    {
                        new LastFmChartTrack() { Title = "WANNABE", Artist = "ITZY", Rank = 1 },
                        new LastFmChartTrack() { Title = "Pretty Please", Artist = "Dua Lipa", Rank = 2 },
                        new LastFmChartTrack() { Title = "Cool", Artist = "Dua Lipa", Rank = 3 },
                    }
                })
                .ReturnsAsync(new LastFmChart<LastFmChartTrack>()
                {
                    Entries = new List<LastFmChartTrack>()
                    {
                        new LastFmChartTrack() { Title = "WANNABE", Artist = "ITZY", Rank = 1 },
                        new LastFmChartTrack() { Title = "Pretty Please", Artist = "Dua Lipa", Rank = 2 },
                        new LastFmChartTrack() { Title = "Hallucinate", Artist = "Dua Lipa", Rank = 3 },
                        new LastFmChartTrack() { Title = "Cool", Artist = "Dua Lipa", Rank = 4 },
                    }
                });

            lastFmMock
                .Setup(s => s.GetWeeklyAlbumChart(anyString, anyLong, anyLong))
                .ReturnsAsync(new LastFmChart<LastFmChartAlbum>()
                {
                    Entries = new List<LastFmChartAlbum>()
                });

            lastFmMock
                .Setup(s => s.GetWeeklyArtistChart(anyString, anyLong, anyLong))
                .ReturnsAsync(new LastFmChart<LastFmChartArtist>()
                {
                    Entries = new List<LastFmChartArtist>()
                });

            var context = ThreeChartsTestContext.BuildInMemoryContext();
            _service = new ChartWeekService(context, lastFmMock.Object);
        }

        public async Task GetStatsForChart_ReturnsCorrectStats()
        {
            var weeks = new List<ChartWeek>();
            for (int i = 0; i < 3; i++)
            {
                var week = new ChartWeek();
                week.Owner = new User() { UserName = "edxds" };

                await _service.CreateEntriesForChartWeek(week);
            }

            var firstWeekResults = weeks[0].ChartEntries.Select(entry
                => _service.GetStatsForChartEntry(entry, weeks)).ToList();

            var secondWeekResults = weeks[1].ChartEntries.Select(entry
                => _service.GetStatsForChartEntry(entry, weeks)).ToList();

            var thirdWeekResults = weeks[2].ChartEntries.Select(entry
                => _service.GetStatsForChartEntry(entry, weeks)).ToList();

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