using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using ThreeChartsAPI.Models;
using ThreeChartsAPI.Services;
using Xunit;

namespace ThreeChartsAPI.Tests
{
    public class ChartWeekService_GetChartWeeksInDateRangeShould
    {
        private readonly ChartWeekService _service;

        public ChartWeekService_GetChartWeeksInDateRangeShould()
        {
            var context = ThreeChartsTestContext.BuildInMemoryContext();
            _service = new ChartWeekService(context);
        }

        [Fact]
        public void GetChartWeeksInDateRange_Start0313End0327_ReturnsCorrectWeeks()
        {
            var startDate = new DateTime(2020, 3, 13, 19, 20, 22);
            var endDate = new DateTime(2020, 3, 27, 15, 23, 44);

            var expectedWeeks = new List<ChartWeek>()
            {
                new ChartWeek()
                {
                    WeekNumber = 1,
                    From = new DateTime(2020, 3, 13),
                    To = new DateTime(2020, 3, 19, 23, 59, 59),
                },
                new ChartWeek()
                {
                    WeekNumber = 2,
                    From = new DateTime(2020, 3, 20),
                    To = new DateTime(2020, 3, 26, 23, 59, 59),
                },
            };

            var actualWeeks = _service.GetChartWeeksInDateRange(1, startDate, endDate);
            actualWeeks.Should().BeEquivalentTo(expectedWeeks);
        }

        [Fact]
        public void GetChartWeeksInDateRange_Start0303End0327_ReturnsCorrectWeeks()
        {
            var startDate = new DateTime(2020, 3, 3, 15, 22, 15);
            var endDate = new DateTime(2020, 3, 27, 22, 48, 19);

            var expectedWeeks = new List<ChartWeek>()
            {
                new ChartWeek()
                {
                    WeekNumber = 1,
                    From = new DateTime(2020, 3, 6),
                    To = new DateTime(2020, 3, 12, 23, 59, 59)
                },
                new ChartWeek()
                {
                    WeekNumber = 2,
                    From = new DateTime(2020, 3, 13),
                    To = new DateTime(2020, 3, 19, 23, 59, 59)
                },
                new ChartWeek()
                {
                    WeekNumber = 3,
                    From = new DateTime(2020, 3, 20),
                    To = new DateTime(2020, 3, 26, 23, 59, 59)
                },
            };

            var actualWeeks = _service.GetChartWeeksInDateRange(1, startDate, endDate);
            actualWeeks.Should().BeEquivalentTo(expectedWeeks);
        }

        [Fact]
        public void GetChartWeeksInDateRange_Start0303End0310_ReturnsNoWeeks()
        {
            var startDate = new DateTime(2020, 3, 3);
            var endDate = new DateTime(2020, 3, 10);

            var expectedWeeks = new List<ChartWeek>();

            var actualWeeks = _service.GetChartWeeksInDateRange(1, startDate, endDate);
            actualWeeks.Should().BeEquivalentTo(expectedWeeks);
        }
    }
}
