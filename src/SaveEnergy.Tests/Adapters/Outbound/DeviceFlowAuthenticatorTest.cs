using System.Net;
using FluentAssertions;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Logging;
using SaveEnergy.Adapters.Outbound;
using SaveEnergy.Domain;
using Xunit.Abstractions;

namespace SaveEnergy.Tests.Adapters.Outbound;

public class DeviceFlowAuthenticatorTest
{
    private readonly ILogger<DeviceFlowAuthenticator> _logger;

    public DeviceFlowAuthenticatorTest(ITestOutputHelper testOutputHelper)
    {
        _logger = XUnitLogger.CreateLogger<DeviceFlowAuthenticator>(testOutputHelper);
    }

    [Fact]
    public async Task RequestAccessToken_HttpClientReturnsEmptyResponse_ThrowsFatalErrorException()
    {
        var authenticator = new DeviceFlowAuthenticator(
            _logger,
            new EmptyResponseHttpClientFactory(),
            new EmptyConfiguration()
        );

        var act = () => authenticator.RequestAccessToken();

        await act.Should().ThrowAsync<FatalErrorException>();
    }

    private class EmptyResponseHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return new HttpClient(new EmptyResponseReturningHandler());
        }

        private class EmptyResponseReturningHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken
            )
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
            }
        }
    }
}
