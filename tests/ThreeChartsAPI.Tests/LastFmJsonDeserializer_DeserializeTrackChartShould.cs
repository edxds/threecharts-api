using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using ThreeChartsAPI.Models.LastFm;
using ThreeChartsAPI.Services.LastFm;
using Xunit;

namespace ThreeChartsAPI.Tests
{
    public class LastFmJsonDeserializer_DeserializeTrackChartShould
    {
        [Fact]
        public async Task DeserializeTrackChart_WithLastFmJson_ReturnsCorrectObject()
        {
            var jsonString = LastFmJsonDeserializerData.WeeklyTrackChartJson;
            var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            var deserializer = new LastFmJsonDeserializer();

            var expectedObject = new LastFmChart<LastFmChartTrack>()
            {
                User = "edxds",
                From = 1584662400,
                To = 1585267199,
                Entries = new List<LastFmChartTrack>()
                {
                    new LastFmChartTrack()
                    {
                        Url = "https://www.last.fm/music/ITZY/_/WANNABE",
                        Artist = "ITZY",
                        Title = "WANNABE",
                        Rank = 1,
                        PlayCount = 29
                    },
                    new LastFmChartTrack()
                    {
                        Url = "https://www.last.fm/music/Pabllo+Vittar/_/Rajad%C3%A3o",
                        Artist = "Pabllo Vittar",
                        Title = "Rajadão",
                        Rank = 2,
                        PlayCount = 29
                    },
                }
            };

            var actualObject = await deserializer.DeserializeTrackChart(jsonStream);
            actualObject.Should().BeEquivalentTo(expectedObject);
        }
    }
}