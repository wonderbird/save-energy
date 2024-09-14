using System.Net;
using FluentAssertions;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
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

    private class EmptyConfiguration : IConfiguration
    {
        public IConfigurationSection GetSection(string key)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            throw new NotImplementedException();
        }

        public IChangeToken GetReloadToken()
        {
            throw new NotImplementedException();
        }

        public string? this[string key]
        {
            get => null;
            set => throw new NotImplementedException();
        }
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
