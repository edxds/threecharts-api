using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using ThreeChartsAPI.Features.LastFm;
using ThreeChartsAPI.Features.LastFm.Models;
using Xunit;

namespace ThreeChartsAPI.Tests
{
    public class LastFmJsonDeserializer_DeserializeSessionShould
    {
        [Fact]
        public async Task DeserializeSession_WithValidJson_ReturnsCorrectSession()
        {
            var jsonString = LastFmJsonDeserializerData.SessionJson;
            var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            var deserializer = new LastFmJsonDeserializer();

            var expectedSession = new LastFmSession()
            {
                LastFmUser = "edxds",
                Key = "session_key",
            };

            var actualSession = await deserializer.DeserializeSession(jsonStream);

            actualSession.Should().BeEquivalentTo(expectedSession);
        }
    }
}