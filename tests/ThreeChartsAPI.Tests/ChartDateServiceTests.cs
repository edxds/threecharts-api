using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using ThreeChartsAPI.Features.Charts;
using ThreeChartsAPI.Features.Charts.Models;
using Xunit;

namespace ThreeChartsAPI.Tests
{
    public class ChartDateServiceTests
    {
        [Fact]
        public void GetChartWeeksInDateRange_Start0313End0327_ReturnsCorrectWeeks()
        {
            // Arrange
            var context = FakeThreeChartsContext.BuildInMemoryContext();
            var service = new ChartDateService(context);

            var startDate = new DateTime(2020, 3, 13, 19, 20, 22);
            var endDate = new DateTime(2020, 3, 27, 15, 23, 44);

            // Act
            var actualWeeks =
                service.GetChartWeeksInDateRange(1, startDate, endDate, TimeZoneInfo.Utc);
            
            // Assert
            var expectedWeeks = new List<ChartWeek>
            {
                new ChartWeek
                {
                    WeekNumber = 1,
                    From = new DateTime(2020, 3, 13),
                    To = new DateTime(2020, 3, 19, 23, 59, 59),
                },
                new ChartWeek
                {
                    WeekNumber = 2,
                    From = new DateTime(2020, 3, 20),
                    To = new DateTime(2020, 3, 26, 23, 59, 59),
                },
            };

            actualWeeks.Should().BeEquivalentTo(expectedWeeks);
        }

        [Fact]
        public void GetChartWeeksInDateRange_Start0303End0327_ReturnsCorrectWeeks()
        {
            // Arrange
            var context = FakeThreeChartsContext.BuildInMemoryContext();
            var service = new ChartDateService(context);

            var startDate = new DateTime(2020, 3, 3, 15, 22, 15);
            var endDate = new DateTime(2020, 3, 27, 22, 48, 19);
            
            // Act
            var actualWeeks =
                service.GetChartWeeksInDateRange(1, startDate, endDate, TimeZoneInfo.Utc);

            // Assert
            var expectedWeeks = new List<ChartWeek>()
            {
                new ChartWeek
                {
                    WeekNumber = 1,
                    From = new DateTime(2020, 3, 3, 0, 0, 0),
                    To = new DateTime(2020, 3, 5, 23, 59, 59),
                },
                new ChartWeek
                {
                    WeekNumber = 2,
                    From = new DateTime(2020, 3, 6),
                    To = new DateTime(2020, 3, 12, 23, 59, 59)
                },
                new ChartWeek
                {
                    WeekNumber = 3,
                    From = new DateTime(2020, 3, 13),
                    To = new DateTime(2020, 3, 19, 23, 59, 59)
                },
                new ChartWeek
                {
                    WeekNumber = 4,
                    From = new DateTime(2020, 3, 20),
                    To = new DateTime(2020, 3, 26, 23, 59, 59)
                },
            };

            actualWeeks.Should().BeEquivalentTo(expectedWeeks);
        }

        [Fact]
        public void GetChartWeeksInDateRange_Start0303End0310_ReturnsOneWeek()
        {
            // Arrange
            var context = FakeThreeChartsContext.BuildInMemoryContext();
            var service = new ChartDateService(context);
            
            var startDate = new DateTime(2020, 3, 3);
            var endDate = new DateTime(2020, 3, 10);

            // Act
            var actualWeeks =
                service.GetChartWeeksInDateRange(1, startDate, endDate, TimeZoneInfo.Utc);
            
            // Assert
            var expectedWeeks = new List<ChartWeek>
            {
                new ChartWeek
                {
                    WeekNumber = 1,
                    From = new DateTime(2020, 3, 3, 0, 0, 0),
                    To = new DateTime(2020, 3, 5, 23, 59, 59),
                }
            };

            actualWeeks.Should().BeEquivalentTo(expectedWeeks);
        }
    }
}
