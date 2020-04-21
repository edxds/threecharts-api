using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentResults;
using ThreeChartsAPI.Features.Charts;
using ThreeChartsAPI.Features.Charts.Models;
using ThreeChartsAPI.Features.LastFm;
using ThreeChartsAPI.Features.LastFm.Models;
using ThreeChartsAPI.Features.Users.Models;

namespace ThreeChartsAPI.Tests
{
    public class OnboardingService_SyncWeeksShould
    {
        private readonly ThreeChartsContext context = ThreeChartsTestContext.BuildInMemoryContext();

        private readonly ChartWeekService _chartWeekService;
        private readonly OnboardingService _onboardingService;

        private readonly DateTime userRegisterDate = new DateTime(2020, 3, 6);
        private readonly DateTime nowDate = new DateTime(2020, 3, 13);

        public OnboardingService_SyncWeeksShould()
        {
            var lastFmMock = new Mock<ILastFmService>();
            lastFmMock
                .Setup(s => s.GetWeeklyTrackChart(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>()))
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

            _chartWeekService = new ChartWeekService(context);
            _onboardingService = new OnboardingService(context, _chartWeekService, lastFmMock.Object);
        }

        [Fact]
        public async Task SyncWeeks_WithGenericUser_SavesWeeksCorrectly()
        {
            var user = new User() { UserName = "edxds", RegisteredAt = userRegisterDate };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            await _onboardingService.SyncWeeks(user, 1, user.RegisteredAt, nowDate, TimeZoneInfo.Utc);

            var actualWeeks = await context.ChartWeeks
                .Where(week => week.OwnerId == user.Id)
                .ToListAsync();

            actualWeeks.Should().HaveCount(1);

            actualWeeks[0].Owner.Should().BeEquivalentTo(user);
            actualWeeks[0].From.Should().Be(new DateTime(2020, 3, 6));
            actualWeeks[0].To.Should().Be(new DateTime(2020, 3, 12, 23, 59, 59));

            actualWeeks[0].ChartEntries.Should().HaveCount(4);

            for (int i = 0; i < actualWeeks[0].ChartEntries.Count; i++)
            {
                // All entries on first week should be new
                var entry = actualWeeks[0].ChartEntries[i];
                var rank = i + 1;

                entry.Rank.Should().Be(rank);
                entry.Stat.Should().Be(ChartEntryStat.New);
            }

            actualWeeks[0].ChartEntries[0].Track.Title.Should().Be("Cool");
            actualWeeks[0].ChartEntries[0].Track.ArtistName.Should().Be("Dua Lipa");

            actualWeeks[0].ChartEntries[1].Track.Title.Should().Be("Pretty Please");
            actualWeeks[0].ChartEntries[1].Track.ArtistName.Should().Be("Dua Lipa");

            actualWeeks[0].ChartEntries[2].Track.Title.Should().Be("Hallucinate");
            actualWeeks[0].ChartEntries[2].Track.ArtistName.Should().Be("Dua Lipa");

            actualWeeks[0].ChartEntries[3].Track.Title.Should().Be("WANNABE");
            actualWeeks[0].ChartEntries[3].Track.ArtistName.Should().Be("ITZY");
        }
    }
}