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
    public class LastFmJsonDeserializer_DeserializeAlbumChartShould
    {
        [Fact]
        public async Task DeserializeAlbumChart_WithLastFmJson_ReturnsCorrectObject()
        {
            var jsonString = LastFmJsonDeserializerData.WeeklyAlbumChartJson;
            var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            var deserializer = new LastFmJsonDeserializer();

            var expectedObject = new LastFmChart<LastFmChartAlbum>()
            {
                User = "edxds",
                From = 1584662400,
                To = 1585267199,
                Entries = new List<LastFmChartAlbum>()
                {
                    new LastFmChartAlbum()
                    {
                        Url = "https://www.last.fm/music/Dua+Lipa/Future+Nostalgia",
                        Artist = "Dua Lipa",
                        Title = "Future Nostalgia",
                        Rank = 1,
                        PlayCount = 94
                    },
                    new LastFmChartAlbum()
                    {
                        Url = "https://www.last.fm/music/Pabllo+Vittar/111",
                        Artist = "Pabllo Vittar",
                        Title = "111",
                        Rank = 2,
                        PlayCount = 93
                    },
                }
            };

            var actualObject = await deserializer.DeserializeAlbumChart(jsonStream);
            actualObject.Should().BeEquivalentTo(expectedObject);
        }
    }
}