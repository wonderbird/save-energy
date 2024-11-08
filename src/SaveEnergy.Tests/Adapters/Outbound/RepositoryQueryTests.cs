using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
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
        var query = new RepositoriesQuery(
            _logger,
            new UnauthorizedHttpClientFactory(),
            new EmptyConfiguration(),
            new FakeAuthenticator()
        );

        var act = () => query.Execute();

        await act.Should().ThrowAsync<FatalErrorException>();
    }

    [Fact]
    public async Task Execute_ResponseContainsNullRequestMessage_DoesNotThrow()
    {
        var query = new RepositoriesQuery(
            _logger,
            new NullRequestMessageHttpClientFactory(),
            new EmptyConfiguration(),
            new FakeAuthenticator()
        );

        var act = () => query.Execute();

        await act.Should().NotThrowAsync();
    }

    [Theory]
    [InlineData("available == page size", 7, 7)]
    [InlineData("available > page size, multiple", 12, 6)]
    [InlineData("available > page size, but not multiple", 12, 5)]
    public async Task Execute_VariyingPageSize_ReturnsAllAvailableRepositories(
        string _,
        int availableRepositories,
        int requestedPageSize
    )
    {
        var fakeHttpClientFactory = new RepositoriesReturningHttpClientFactory(
            availableRepositories
        );

        var query = new RepositoriesQuery(
            _logger,
            fakeHttpClientFactory,
            new EmptyConfiguration(),
            new FakeAuthenticator()
        )
        {
            RequestedPageSize = requestedPageSize
        };

        var actual = await query.Execute();

        actual.Should().HaveCount(availableRepositories);
    }

    [Fact]
    public async Task Execute_AvailableLargerPageSizeButNotMultiple_DoesNotRequestInvalidPage()
    {
        const int availableRepositories = 12;
        const int requestedPageSize = 5;

        var fakeHttpClientFactory = new RepositoriesReturningHttpClientFactory(
            availableRepositories
        );

        var query = new RepositoriesQuery(
            _logger,
            fakeHttpClientFactory,
            new EmptyConfiguration(),
            new FakeAuthenticator()
        )
        {
            RequestedPageSize = requestedPageSize
        };

        await query.Execute();

        fakeHttpClientFactory.RequestedUrls.Should().NotContain(url => url.Contains("page=4"));
        fakeHttpClientFactory.RequestedUrls.Should().HaveCount(3);
    }

    private class FakeAuthenticator : ICanAuthenticate
    {
        public Task<AccessToken> RequestAccessToken()
        {
            return Task.FromResult(new AccessToken("FakeTokenFromFakeAuthenticator"));
        }
    }

    private class UnauthorizedHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return new HttpClient(new UnauthorizedResponseReturningHandler());
        }

        private class UnauthorizedResponseReturningHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken
            )
            {
                var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);

                response.RequestMessage = new HttpRequestMessage(
                    request.Method,
                    request.RequestUri
                );

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
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken
            )
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);

                response.RequestMessage = new HttpRequestMessage();
                response.Content = new StringContent("[]");

                return Task.FromResult(response);
            }
        }
    }

    private class RepositoriesReturningHttpClientFactory(int availableRepositories)
        : IHttpClientFactory
    {
        public BlockingCollection<string> RequestedUrls { get; } = [];

        public HttpClient CreateClient(string name)
        {
            return new HttpClient(
                new RepositoriesReturningHandler(availableRepositories, RequestedUrls)
            );
        }

        private class RepositoriesReturningHandler(
            int availableRepositories,
            BlockingCollection<string> requestedUrls
        ) : HttpMessageHandler
        {
            /// <summary>
            /// Provide repositories for requested page and page size.
            /// </summary>
            ///
            /// <remarks>
            /// <para>
            /// The function mimics the behavior of the GitHub for pages larger than the number of
            /// available repositories. In this case, the function returns an empty list of repositories.
            /// The behavior of the GitHub API can be verified using the HTTP requests in
            /// ./docs/github-rest-api.http
            /// </para>
            ///
            /// <para>
            /// This behavior is actually coded in the <see cref="GetPageIntervalFromRequest"/> function.
            /// </para>
            /// </remarks>
            ///
            /// <param name="request">GitHub API request containing the parameters "page" and "per_page"</param>
            /// <param name="cancellationToken">Allows cancelling the asynchronous operations</param>
            ///
            /// <returns>HTTP response containing list of repositories for the requested page</returns>
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken
            )
            {
                requestedUrls.Add(request.RequestUri!.ToString(), cancellationToken);

                var response = new HttpResponseMessage(HttpStatusCode.OK);

                response.RequestMessage = new HttpRequestMessage(
                    request.Method,
                    request.RequestUri
                );

                var (start, count) = GetPageIntervalFromRequest(request);

                var repositories = Enumerable
                    .Range(start, count)
                    .Select(x => new Repository(
                        $"Repository-{x}",
                        DateTime.UtcNow,
                        $"Description of repository {x}",
                        $"http://example.com/repository-{x}",
                        $"git@example.com:some-user/repository-{x}.git",
                        $"https://example.com/some-user/repository-{x}.git"
                    ));

                response.Content = new StringContent(JsonSerializer.Serialize(repositories));

                return Task.FromResult(response);
            }

            /// <summary>
            /// Parse start index and number of requested repositories from the request URI.
            /// </summary>
            ///
            /// <remarks>
            /// If the requested page is larger than the number of available repositories,
            /// the function sets the count return value to 0.
            /// </remarks>
            ///
            /// <param name="request">HTTP request for a page of repositories</param>
            ///
            /// <returns>
            /// Tuple of (start, count) where start is the index of the first repository in the list
            /// of all repositories and count is the number of requested repositories.
            /// </returns>
            private (int start, int count) GetPageIntervalFromRequest(HttpRequestMessage request)
            {
                const char separator = '&';

                var queryString = request.RequestUri?.Query[1..] ?? "";
                var queryParameters = queryString.Split(separator);

                var page = ParseQueryParameter("page=", queryParameters);
                var perPage = ParseQueryParameter("per_page=", queryParameters);
                
                var start = (page - 1) * perPage + 1;
                var count = Math.Min(perPage, availableRepositories - start + 1);
                count = Math.Max(count, 0);

                return (start, count);
            }

            private static int ParseQueryParameter(string parameterName, string[] queryParameters)
            {
                var page = int.Parse(
                    Array
                        .Find(
                            queryParameters,
                            x => x.StartsWith(parameterName, StringComparison.OrdinalIgnoreCase)
                        )
                        ?.Split('=')[1] ?? "1"
                );
                return page;
            }
        }
    }
}
