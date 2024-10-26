using System.Net;
using FluentAssertions;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Logging;
using SaveEnergy.Adapters.Outbound;
using SaveEnergy.Domain;
using Xunit.Abstractions;

namespace SaveEnergy.Tests.Adapters.Outbound;

public class RepositoryQueryTests
{
    private readonly ILogger<RepositoriesQuery> _logger;

    public RepositoryQueryTests(ITestOutputHelper testOutputHelper)
    {
        _logger = XUnitLogger.CreateLogger<RepositoriesQuery>(testOutputHelper);
    }
    
    [Fact]
    public async Task Execute_InvalidAccessException_ThrowsFatalErrorException()
    {
        var query = new RepositoriesQuery(_logger, new UnauthorizedHttpClientFactory(), new EmptyConfiguration(), new FakeAuthenticator());
        
        var act = () => query.Execute();

        await act.Should().ThrowAsync<FatalErrorException>();
    }

    [Fact]
    public async Task Execute_ResponseContainsNullRequestMessage_DoesNotThrow()
    {
        var query = new RepositoriesQuery(_logger, new NullRequestMessageHttpClientFactory(), new EmptyConfiguration(), new FakeAuthenticator());
        
        var act = () => query.Execute();

        await act.Should().NotThrowAsync();
    }
    
    private class UnauthorizedHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return new HttpClient(new UnauthorizedResponseReturningHandler());
        }

        private class UnauthorizedResponseReturningHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                
                response.RequestMessage = new HttpRequestMessage(request.Method, request.RequestUri);
                
                return Task.FromResult(response);
            }
        }
    }

    private class NullRequestMessageHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return new HttpClient(new NullRequestMessageReturningHandler());
        }

        private class NullRequestMessageReturningHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                
                response.RequestMessage = new HttpRequestMessage();
                response.Content = new StringContent("[]");
                
                return Task.FromResult(response);
            }
        }
    }
    
    private class FakeAuthenticator : ICanAuthenticate
    {
        public Task<AccessToken> RequestAccessToken()
        {
            return Task.FromResult(new AccessToken("FakeTokenFromFakeAuthenticator"));
        }
    }
}
