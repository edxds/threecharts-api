using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using ThreeChartsAPI.Features.LastFm;
using ThreeChartsAPI.Features.LastFm.Models;
using Xunit;

namespace ThreeChartsAPI.Tests
{
    public class LastFmJsonDeserializerTests
    {
        [Fact]
        public async Task DeserializeAlbumChart_WithValidJson_ReturnsCorrectObject()
        {
            // Arrange
            var deserializer = new LastFmJsonDeserializer();
            var jsonStream = MakeJsonStreamFrom(LastFmJsonDeserializerData.WeeklyAlbumChartJson);

            // Act
            var actualObject = await deserializer.DeserializeAlbumChart(jsonStream);
            
            // Assert
            var expectedObject = new LastFmChart<LastFmChartAlbum>()
            {
                User = "edxds",
                From = 1584662400,
                To = 1585267199,
                Entries = new List<LastFmChartAlbum>
                {
                    new LastFmChartAlbum
                    {
                        Url = "https://www.last.fm/music/Dua+Lipa/Future+Nostalgia",
                        Artist = "Dua Lipa",
                        Title = "Future Nostalgia",
                        Rank = 1,
                        PlayCount = 94
                    },
                    new LastFmChartAlbum
                    {
                        Url = "https://www.last.fm/music/Pabllo+Vittar/111",
                        Artist = "Pabllo Vittar",
                        Title = "111",
                        Rank = 2,
                        PlayCount = 93
                    },
                }
            };
            
            actualObject.Should().BeEquivalentTo(expectedObject);
        }

        [Fact]
        public async Task DeserializeArtistChart_WithValidJson_ReturnsCorrectObject()
        {
            // Arrange
            var deserializer = new LastFmJsonDeserializer();
            var jsonStream = MakeJsonStreamFrom(LastFmJsonDeserializerData.WeeklyArtistChartJson);
            
            // Act
            var actualObject = await deserializer.DeserializeArtistChart(jsonStream);
            
            // Assert
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
            
            actualObject.Should().BeEquivalentTo(expectedObject);
        }
        
        [Fact]
        public async Task DeserializeTrackChart_WithValidJson_ReturnsCorrectObject()
        {
            // Arrange
            var deserializer = new LastFmJsonDeserializer();
            var jsonStream = MakeJsonStreamFrom(LastFmJsonDeserializerData.WeeklyTrackChartJson);

            // Act
            var actualObject = await deserializer.DeserializeTrackChart(jsonStream);
            
            // Assert
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
            
            actualObject.Should().BeEquivalentTo(expectedObject);
        }
        
        [Fact]
        public async Task DeserializeSession_WithValidJson_ReturnsCorrectSession()
        {
            // Arrange
            var deserializer = new LastFmJsonDeserializer();
            var jsonStream = MakeJsonStreamFrom(LastFmJsonDeserializerData.SessionJson);

            // Act
            var actualSession = await deserializer.DeserializeSession(jsonStream);
            
            // Assert
            var expectedSession = new LastFmSession()
            {
                LastFmUser = "edxds",
                Key = "session_key",
            };

            actualSession.Should().BeEquivalentTo(expectedSession);
        }
        
        [Fact]
        public async Task DeserializeUserInfo_WithValidJson_ReturnsCorrectUserInfo()
        {
            // Arrange
            var deserializer = new LastFmJsonDeserializer();
            var jsonStream = MakeJsonStreamFrom(LastFmJsonDeserializerData.UserInfoJson);

            // Act
            var actualUserInfo = await deserializer.DeserializeUserInfo(jsonStream);

            // Assert
            var expectedUserInfo = new LastFmUserInfo()
            {
                Name = "edxds",
                RealName = "",
                Url = "https://www.last.fm/user/edxds",
                Image = "https://lastfm.freetls.fastly.net/i/u/300x300/e4612070134f0e1b58968471969d54da.png",
                RegisterDate = 1501983583,
            };
            
            actualUserInfo.Should().BeEquivalentTo(expectedUserInfo);
        }
        
        private Stream MakeJsonStreamFrom(string fromValue)
        {
            var jsonString = fromValue;
            var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));

            return jsonStream;
        } 
    }
}