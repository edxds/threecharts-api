using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using ThreeChartsAPI.Features.LastFm;
using ThreeChartsAPI.Features.LastFm.Models;
using Xunit;

namespace ThreeChartsAPI.Tests
{
    public class LastFmJsonDeserializer_DeserializeUserInfoShould
    {
        [Fact]
        public async Task DeserializeUserInfo_WithValidJson_ReturnsCorrectUserInfo()
        {
            var jsonString = LastFmJsonDeserializerData.UserInfoJson;
            var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            var deserializer = new LastFmJsonDeserializer();

            var expectedUserInfo = new LastFmUserInfo()
            {
                Name = "edxds",
                RealName = "",
                Url = "https://www.last.fm/user/edxds",
                Image = "https://lastfm.freetls.fastly.net/i/u/300x300/e4612070134f0e1b58968471969d54da.png",
                RegisterDate = 1501983583,
            };

            var actualUserInfo = await deserializer.DeserializeUserInfo(jsonStream);

            actualUserInfo.Should().BeEquivalentTo(expectedUserInfo);
        }
    }
}