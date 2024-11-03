using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SaveEnergy.Domain;

namespace SaveEnergy.Adapters.Outbound;

public class RepositoriesQuery : IRepositoriesQuery
{
#pragma warning disable S1075
    /// <summary>
    /// Fallback API base address, if not configured.
    /// </summary>
    ///
    /// <remarks>
    /// <para>
    /// A configurable API base address allows using the
    /// <c>MockServer</c> in the tests.
    /// </para>
    ///
    /// <para>
    /// Warning S1075 (URIs should not be hardcoded) is disabled, because using
    /// a hard-coded URI makes the program easier to use.
    /// </para>
    ///
    /// <para>
    /// Without a default URI, the program would show an error and stop if the
    /// user hasn't set one. The user would then need to set the GitHub URL and
    /// restart the program.
    /// </para>
    /// </remarks>
    ///
    /// <seealso cref="DeviceFlowAuthenticator.DefaultAuthenticationBaseAddress"/>
    private const string DefaultApiBaseAddress = "https://api.github.com";
#pragma warning restore S1075

    private readonly ILogger<RepositoriesQuery> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ICanAuthenticate _authenticator;
    private readonly HttpClient _apiClient;
    internal int RequestedPageSize = 100;

    public RepositoriesQuery(
        ILogger<RepositoriesQuery> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ICanAuthenticate authenticator
    )
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _authenticator = authenticator;

        _apiClient = CreateApiClient();
    }

    private HttpClient CreateApiClient()
    {
        var result = _httpClientFactory.CreateClient();

        result.BaseAddress = new Uri(
            _configuration["GitHub:ApiBaseAddress"] ?? DefaultApiBaseAddress
        );

        result.DefaultRequestHeaders.Add("Accept", "application/json");
        result.DefaultRequestHeaders.Add("User-Agent", "SaveEnergy");

        _logger.LogDebug("API base address: {ApiBaseAddress}", result.BaseAddress);

        return result;
    }

    public async Task<IEnumerable<Repository>> Execute()
    {
        try
        {
            var accessTokenResponse = await _authenticator.RequestAccessToken();
            _apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                accessTokenResponse.Token
            );

            var incrementallyBuiltUpRepositories = new List<Repository>();
            bool hasMoreRepositories;
            int page = 1;

            do
            {
                using var undecodedRepositoriesResponse = await _apiClient.GetAsync(
                    $"/user/repos?affiliation=owner&sort=pushed&direction=desc&per_page={RequestedPageSize}&page={page}"
                );

                // TODO: Is it helpful to log the final request URI after having received the response? Wouldn't it be more helpful to log the request URI before sending the request? (Originally this log was used for debugging, but this is not necessary, because we now know how to trace the requests.). Find similar places.
                _logger.LogDebug(
                    "Final request URI: {RequestUri}",
                    undecodedRepositoriesResponse.RequestMessage?.RequestUri
                );

                undecodedRepositoriesResponse.EnsureSuccessStatusCode();

                var maybeRepositories =
                    await undecodedRepositoriesResponse.Content.ReadFromJsonAsync<
                        IEnumerable<Repository>
                    >();
                var repositories = maybeRepositories?.ToList() ?? [];

                if (repositories.Any())
                {
                    incrementallyBuiltUpRepositories.AddRange(repositories);
                }

                hasMoreRepositories = repositories.Count == RequestedPageSize;
                ++page;
            } while (hasMoreRepositories);

            return incrementallyBuiltUpRepositories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve repositories from GitHub");
            throw new FatalErrorException();
        }
    }
}
