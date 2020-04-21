using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Moq;
using Moq.Protected;
using ThreeChartsAPI.Features.LastFm;
using ThreeChartsAPI.Features.LastFm.Models;
using Xunit;

namespace ThreeChartsAPI.Tests
{
    public class HttpLastFmService_GetWeeklyArtistChartShould
    {
        [Fact]
        public async Task GetWeeklyArtistChart_CallsHttpClientCorrectly()
        {
            var baseUri = "http://ws.audioscrobbler.com/2.0/";

            var deserializerMock = new Mock<ILastFmDeserializer>();
            deserializerMock.Setup(d => d.DeserializeArtistChart(It.IsAny<Stream>()))
                .ReturnsAsync(new LastFmChart<LastFmChartArtist>());

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(""),
                })
                .Verifiable();

            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.CreateClient("lastFm"))
                .Returns(new HttpClient(handlerMock.Object) { BaseAddress = new Uri(baseUri) });

            var service = new HttpLastFmService(
                factoryMock.Object,
                deserializerMock.Object,
                new HttpLastFmService.Settings("key", "secret")
            );

            var expectedApiKey = "key";
            var expectedUser = "edxds";
            var expectedFrom = 0;
            var expectedTo = 0;
            var expectedRequestUri =
                $"{baseUri}?method=user.getweeklyartistchart&api_key={expectedApiKey}&user={expectedUser}&from={expectedFrom}&to={expectedTo}&format=json";

            await service.GetWeeklyArtistChart("edxds", 0, 0);

            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req
                    => req.Method == HttpMethod.Get
                        && req.RequestUri.ToString() == expectedRequestUri
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task GetWeeklyArtistChart_HandlesKnownErrorsCorrectly()
        {
            var baseUri = "http://ws.audioscrobbler.com/2.0/";

            var deserializerMock = new Mock<ILastFmDeserializer>();
            deserializerMock.Setup(d => d.DeserializeError(It.IsAny<Stream>()))
                .ReturnsAsync(new LastFmError() { ErrorCode = 1, Message = "Message" });

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.Forbidden,
                    Content = new StringContent(""),
                });

            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.CreateClient("lastFm"))
                .Returns(new HttpClient(handlerMock.Object) { BaseAddress = new Uri(baseUri) });

            var service = new HttpLastFmService(
                factoryMock.Object,
                deserializerMock.Object,
                new HttpLastFmService.Settings("key", "secret")
            );

            var result = await service.GetWeeklyArtistChart("edxds", 0, 0);

            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainEquivalentOf(
                new LastFmResultError(
                    (int)HttpStatusCode.Forbidden,
                    new LastFmError() { ErrorCode = 1, Message = "Message" }
                )
            );
        }

        [Fact]
        public async Task GetWeeklyArtistChart_HandlesUnknownErrorsCorrectly()
        {
            var baseUri = "http://ws.audioscrobbler.com/2.0/";
            var exception = new JsonException();

            var deserializerMock = new Mock<ILastFmDeserializer>();
            deserializerMock.Setup(d => d.DeserializeError(It.IsAny<Stream>()))
                .ThrowsAsync(exception);

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.Forbidden,
                    Content = new StringContent(""),
                });

            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.CreateClient("lastFm"))
                .Returns(new HttpClient(handlerMock.Object) { BaseAddress = new Uri(baseUri) });

            var service = new HttpLastFmService(
                factoryMock.Object,
                deserializerMock.Object,
                new HttpLastFmService.Settings("key", "secret")
            );

            var result = await service.GetWeeklyArtistChart("edxds", 0, 0);

            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainEquivalentOf(
                new LastFmResultError((int)HttpStatusCode.ServiceUnavailable, null)
                    .CausedBy(exception)
            );
        }
    }
}