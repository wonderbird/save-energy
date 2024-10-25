using System.Net;
using FluentAssertions;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Logging;
using SaveEnergy.Adapters.Outbound;
using SaveEnergy.Domain;
using Xunit.Abstractions;

namespace SaveEnergy.Tests;

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
        var authenticator = new DeviceFlowAuthenticator(new EmptyResponseHttpClientFactory(), new EmptyConfiguration(), _logger);

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
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
            }
        }
    }
}