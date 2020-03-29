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
    public class LastFmJsonDeserializer_DeserializeArtistChartShould
    {
        [Fact]
        public async Task DeserializeArtistChart_WithLastFmJson_ReturnsCorrectObject()
        {
            var jsonString = LastFmJsonDeserializerData.WeeklyArtistChartJson;
            var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            var deserializer = new LastFmJsonDeserializer();

            var expectedObject = new LastFmChart<LastFmChartArtist>()
            {
                User = "edxds",
                From = 1584662400,
                To = 1585267199,
                Entries = new List<LastFmChartArtist>()
                {
                    new LastFmChartArtist()
                    {
                        Url = "https://www.last.fm/music/Pabllo+Vittar",
                        Name = "Pabllo Vittar",
                        Rank = 1,
                        PlayCount = 97
                    },
                    new LastFmChartArtist()
                    {
                        Url = "https://www.last.fm/music/Dua+Lipa",
                        Name = "Dua Lipa",
                        Rank = 2,
                        PlayCount = 94
                    },
                }
            };

            var actualObject = await deserializer.DeserializeArtistChart(jsonStream);
            actualObject.Should().BeEquivalentTo(expectedObject);
        }
    }
}