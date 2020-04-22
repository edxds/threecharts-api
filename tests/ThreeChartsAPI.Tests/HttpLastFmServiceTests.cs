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
    public class HttpLastFmServiceTests
    {
        private const string BaseUri = "http://ws.audioscrobbler.com/2.0/";

        [Fact]
        public async Task CreateLastFmSession_CallsHttpClientCorrectly()
        {
            // Arrange
            var (service, handlerMock) = MakeOkService();
            
            // Act
            await service.CreateLastFmSession("token");

            // Assert
            var expectedApiKey = "key";
            var expectedToken = "token";
            var expectedSignature = "70a95afa7284fa9ebb504a3123c4c7eb";
            var expectedRequestUri =
                $"{BaseUri}?method=auth.getsession&api_key={expectedApiKey}&token={expectedToken}&api_sig={expectedSignature}&format=json";

            VerifyHandlerWasCalledWith(handlerMock, HttpMethod.Get, expectedRequestUri);
        }

        [Fact]
        public async Task CreateLastFmSession_HandlesKnownErrorsCorrectly()
        {
            // Arrange
            var deserializerMock = MakeFakeErrorLastFmDeserializer();
            var handlerMock = MakeFakeForbiddenHttpMessageHandler();
            var factoryMock = MakeFakeHttpClientFactory(handlerMock.Object);

            var service = new HttpLastFmService(
                factoryMock.Object,
                deserializerMock.Object,
                new HttpLastFmService.Settings("key", "secret")
            );

            // Act
            var result = await service.CreateLastFmSession("token");

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainEquivalentOf(
                new LastFmResultError(
                    (int)HttpStatusCode.Forbidden,
                    new LastFmError() { ErrorCode = 1, Message = "Message" }
                )
            );
        }

        [Fact]
        public async Task CreateLastFmSession_HandlesUnknownErrorsCorrectly()
        {
            // Arrange
            var exception = new JsonException();
            var deserializerMock = MakeFakeExceptionLastFmDeserializer(exception);
            var handlerMock = MakeFakeForbiddenHttpMessageHandler();
            var factoryMock = MakeFakeHttpClientFactory(handlerMock.Object);

            var service = new HttpLastFmService(
                factoryMock.Object,
                deserializerMock.Object,
                new HttpLastFmService.Settings("key", "secret")
            );

            // Act
            var result = await service.CreateLastFmSession("token");

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainEquivalentOf(
                new LastFmResultError((int)HttpStatusCode.ServiceUnavailable, null)
                    .CausedBy(exception)
            );
        }
        
        [Fact]
        public async Task GetUserInfoShould_WithSessionKey_CallsHttpClientCorrectly()
        {
            // Arrange
            var (service, handlerMock) = MakeOkService();

            // Act
            await service.GetUserInfo(null, "sk");

            // Assert
            var expectedApiKey = "key";
            var expectedSessionKey = "sk";
            var expectedRequestUri =
                $"{BaseUri}?method=user.getinfo&api_key={expectedApiKey}&sk={expectedSessionKey}&format=json";
            
            VerifyHandlerWasCalledWith(handlerMock, HttpMethod.Get, expectedRequestUri);
            
        }
        
        [Fact]
        public async Task GetWeeklyAlbumChart_CallsHttpClientCorrectly()
        {
            // Arrange
            var (service, handlerMock) = MakeOkService();

            // Act
            await service.GetWeeklyAlbumChart("edxds", 0, 0);
            
            // Assert
            var expectedApiKey = "key";
            var expectedUser = "edxds";
            var expectedFrom = 0;
            var expectedTo = 0;
            var expectedRequestUri =
                $"{BaseUri}?method=user.getweeklyalbumchart&api_key={expectedApiKey}&user={expectedUser}&from={expectedFrom}&to={expectedTo}&format=json";
            
            VerifyHandlerWasCalledWith(handlerMock, HttpMethod.Get, expectedRequestUri);
        }

        [Fact]
        public async Task GetWeeklyArtistChart_CallsHttpClientCorrectly()
        {
            // Arrange
            var (service, handlerMock) = MakeOkService();

            // Act
            await service.GetWeeklyArtistChart("edxds", 0, 0);
            
            // Assert
            var expectedApiKey = "key";
            var expectedUser = "edxds";
            var expectedFrom = 0;
            var expectedTo = 0;
            var expectedRequestUri =
                $"{BaseUri}?method=user.getweeklyartistchart&api_key={expectedApiKey}&user={expectedUser}&from={expectedFrom}&to={expectedTo}&format=json";
            
            VerifyHandlerWasCalledWith(handlerMock, HttpMethod.Get, expectedRequestUri);
        }
        
        [Fact]
        public async Task GetWeeklyTrackChart_CallsHttpClientCorrectly()
        {
            // Arrange
            var (service, handlerMock) = MakeOkService();

            // Act
            await service.GetWeeklyTrackChart("edxds", 0, 0);

            // Assert
            var expectedApiKey = "key";
            var expectedUser = "edxds";
            var expectedFrom = 0;
            var expectedTo = 0;
            var expectedRequestUri =
                $"{BaseUri}?method=user.getweeklytrackchart&api_key={expectedApiKey}&user={expectedUser}&from={expectedFrom}&to={expectedTo}&format=json";
            
            VerifyHandlerWasCalledWith(handlerMock, HttpMethod.Get, expectedRequestUri);
        }
        
        private (HttpLastFmService, Mock<HttpMessageHandler>) MakeOkService()
        {
            var deserializerMock = new Mock<ILastFmDeserializer>();
            var handlerMock = MakeFakeOkHttpMessageHandler();
            var factoryMock = MakeFakeHttpClientFactory(handlerMock.Object);

            var service = new HttpLastFmService(
                factoryMock.Object,
                deserializerMock.Object,
                new HttpLastFmService.Settings("key", "secret")
            );

            return (service, handlerMock);
        }

        private Mock<ILastFmDeserializer> MakeFakeErrorLastFmDeserializer()
        {
            var deserializerMock = new Mock<ILastFmDeserializer>();
            deserializerMock.Setup(d => d.DeserializeError(It.IsAny<Stream>()))
                .ReturnsAsync(new LastFmError() { ErrorCode = 1, Message = "Message" });

            return deserializerMock;
        }

        private Mock<ILastFmDeserializer> MakeFakeExceptionLastFmDeserializer(Exception exception)
        {
            var deserializerMock = new Mock<ILastFmDeserializer>();
            deserializerMock.Setup(d => d.DeserializeError(It.IsAny<Stream>()))
                .ThrowsAsync(exception);

            return deserializerMock;
        }
        
        private Mock<IHttpClientFactory> MakeFakeHttpClientFactory(HttpMessageHandler handler)
        {
            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.CreateClient("lastFm"))
                .Returns(new HttpClient(handler)
                {
                    BaseAddress = new Uri("http://ws.audioscrobbler.com/2.0/")
                });

            return factoryMock;
        }

        private Mock<HttpMessageHandler> MakeFakeOkHttpMessageHandler()
        {
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

            return handlerMock;
        }
        
        private Mock<HttpMessageHandler> MakeFakeForbiddenHttpMessageHandler()
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Forbidden,
                    Content = new StringContent(""),
                })
                .Verifiable();

            return handlerMock;
        }

        private void VerifyHandlerWasCalledWith(
            Mock<HttpMessageHandler> handler,
            HttpMethod method,
            string uri)
        {
            handler.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == method
                    && req.RequestUri.ToString() == uri),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}