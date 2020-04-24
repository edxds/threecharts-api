using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using ThreeChartsAPI.Features.Charts;
using ThreeChartsAPI.Features.Charts.Models;
using ThreeChartsAPI.Features.LastFm.Models;
using ThreeChartsAPI.Features.Users.Models;
using Xunit;

namespace ThreeChartsAPI.Tests
{
    public class ChartServiceTests
    {
        [Fact]
        public async Task GetStatsForChart_WithMultipleTrackEntries_ReturnsCorrectStats()
        {
            // Arrange
            var lastFmFake = new FakeLastFmService();
            var lastFmStub = lastFmFake.Object;
            
            // Represents the tracks that will be returned over time
            var tracksList = new List<List<LastFmChartTrack>>
            {
                new List<LastFmChartTrack>
                {
                    new LastFmChartTrack { Title = "Cool", Artist = "Dua Lipa", Rank = 1 },
                    new LastFmChartTrack { Title = "Pretty Please", Artist = "Dua Lipa", Rank = 2 },
                    new LastFmChartTrack { Title = "Hallucinate", Artist = "Dua Lipa", Rank = 3 },
                    new LastFmChartTrack { Title = "WANNABE", Artist = "ITZY", Rank = 4 },
                },
                new List<LastFmChartTrack>
                {
                    new LastFmChartTrack { Title = "WANNABE", Artist = "ITZY", Rank = 1 },
                    new LastFmChartTrack { Title = "Pretty Please", Artist = "Dua Lipa", Rank = 2 },
                    new LastFmChartTrack { Title = "Cool", Artist = "Dua Lipa", Rank = 3 },
                },
                new List<LastFmChartTrack>
                {
                    new LastFmChartTrack { Title = "WANNABE", Artist = "ITZY", Rank = 1 },
                    new LastFmChartTrack { Title = "Pretty Please", Artist = "Dua Lipa", Rank = 2 },
                    new LastFmChartTrack { Title = "Hallucinate", Artist = "Dua Lipa", Rank = 3 },
                    new LastFmChartTrack { Title = "Cool", Artist = "Dua Lipa", Rank = 4 },
                }
            };

            var context = FakeThreeChartsContext.BuildInMemoryContext();
            var chartDateService = new ChartDateService(context);
            var repo = new ChartRepository(context);
            var chartService = new ChartService(repo, chartDateService, lastFmStub);

            // Act
            var weeks = new List<ChartWeek>();
            for (var i = 0; i < 3; i++)
            {
                var week = new ChartWeek
                {
                    Owner = new User { UserName = "edxds" }, WeekNumber = i + 1
                };

                // Set LastFm fake to return correct tracks according to index
                lastFmFake.Tracks = tracksList[i];
                lastFmFake.SetupFake(); // Updates fake returns
                
                var trackChart = await lastFmStub.GetWeeklyTrackChart("", 0, 0);
                var albumChart = await lastFmStub.GetWeeklyAlbumChart("", 0, 0);
                var artistChart = await lastFmStub.GetWeeklyArtistChart("", 0, 0);

                week.ChartEntries = chartService.CreateEntriesForLastFmCharts(
                    trackChart.Value,
                    albumChart.Value,
                    artistChart.Value,
                    week
                );

                weeks.Add(week);
            }

            var results = weeks
                .Select(w => w.ChartEntries)
                .Select(entries =>
                    entries
                        .Select(entry => chartService.GetStatsForChartEntry(entry, weeks))
                        .ToList())
                .ToList();

            // Assert
            // All stats on the first week should be .New
            results[0].ForEach(r => r.stat.Should().Be(ChartEntryStat.New));

            results[1].Should().BeEquivalentTo(
                new List<(ChartEntryStat stat, string statText)>()
                {
                    (ChartEntryStat.Increase, "+3"),
                    (ChartEntryStat.NoDiff, "="),
                    (ChartEntryStat.Decrease, "-2"),
                }
            );

            results[2].Should().BeEquivalentTo(
                new List<(ChartEntryStat stat, string statText)>()
                {
                    (ChartEntryStat.NoDiff, "="),
                    (ChartEntryStat.NoDiff, "="),
                    (ChartEntryStat.Reentry, null),
                    (ChartEntryStat.Decrease, "-1"),
                }
            );
        }

        [Fact]
        public async Task GetLiveWeeksFor_WithRecentlyRegisteredUser_ShouldCallLastFmCorrectly()
        {
            // Arrange
            var context = FakeThreeChartsContext.BuildInMemoryContext();
            var repo = new ChartRepository(context);
            var lastFmMock = new FakeLastFmService();
            var chartDateService = new ChartDateService(context);
            var service = new ChartService(repo, chartDateService, lastFmMock.Object);
            
            var userRegisterDate = new DateTime(2020, 4, 21);
            var endDate = new DateTime(2020, 4, 23, 23, 59, 59);
            var now = new DateTime(2020, 4, 22);
            
            var user = new User
            {
                UserName = "edxds",
                IanaTimezone = "America/Sao_Paulo",
                RegisteredAt = userRegisterDate,
            };

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
            
            // Act
            await service.GetLiveWeekFor(user, now);
            
            // Assert
            lastFmMock.Fake
                .Verify(lfm => lfm.GetWeeklyTrackChart(
                    It.Is<string>(s => s == "edxds"),
                    It.Is<long>(l => l == new DateTimeOffset(userRegisterDate).ToUnixTimeSeconds()),
                    It.Is<long>(l => l == new DateTimeOffset(endDate).ToUnixTimeSeconds())));
        }
        
        [Fact]
        public async Task SyncWeeks_WithGenericUser_SavesWeeksCorrectly()
        {
            // Arrange
            var context = FakeThreeChartsContext.BuildInMemoryContext();
            var chartDateService = new ChartDateService(context);
            
            var lastFmFake = new FakeLastFmService();
            var lastFmStub = lastFmFake.Object;

            lastFmFake.Tracks = new List<LastFmChartTrack>
            {
                new LastFmChartTrack { Title = "Cool", Artist = "Dua Lipa", Rank = 1 },
                new LastFmChartTrack { Title = "Pretty Please", Artist = "Dua Lipa", Rank = 2 },
                new LastFmChartTrack { Title = "Hallucinate", Artist = "Dua Lipa", Rank = 3 },
                new LastFmChartTrack { Title = "WANNABE", Artist = "ITZY", Rank = 4 },
            };

            lastFmFake.SetupFake();
            var repo = new ChartRepository(context);
            var service = new ChartService(repo, chartDateService, lastFmStub);

            var userRegisterDate = new DateTime(2020, 3, 6);
            var nowDate = new DateTime(2020, 3, 13);
            var user = new User() { UserName = "edxds", RegisteredAt = userRegisterDate };

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            // Act
            await service.SyncWeeks(user, 1, user.RegisteredAt, nowDate, TimeZoneInfo.Utc);

            // Assert
            var actualWeeks = await context.ChartWeeks
                .Where(week => week.OwnerId == user.Id)
                .ToListAsync();

            actualWeeks.Should().HaveCount(1);

            actualWeeks[0].Owner.Should().BeEquivalentTo(user);
            actualWeeks[0].From.Should().Be(new DateTime(2020, 3, 6));
            actualWeeks[0].To.Should().Be(new DateTime(2020, 3, 12, 23, 59, 59));

            actualWeeks[0].ChartEntries.Should().HaveCount(4);

            for (var i = 0; i < actualWeeks[0].ChartEntries.Count; i++)
            {
                // All entries on first week should be new
                var entry = actualWeeks[0].ChartEntries[i];
                var rank = i + 1;

                entry.Rank.Should().Be(rank);
                entry.Stat.Should().Be(ChartEntryStat.New);
            }

            actualWeeks[0].ChartEntries[0].Title.Should().Be("Cool");
            actualWeeks[0].ChartEntries[0].Artist.Should().Be("Dua Lipa");

            actualWeeks[0].ChartEntries[1].Title.Should().Be("Pretty Please");
            actualWeeks[0].ChartEntries[1].Artist.Should().Be("Dua Lipa");

            actualWeeks[0].ChartEntries[2].Title.Should().Be("Hallucinate");
            actualWeeks[0].ChartEntries[2].Artist.Should().Be("Dua Lipa");

            actualWeeks[0].ChartEntries[3].Title.Should().Be("WANNABE");
            actualWeeks[0].ChartEntries[3].Artist.Should().Be("ITZY");
        }
    }
}