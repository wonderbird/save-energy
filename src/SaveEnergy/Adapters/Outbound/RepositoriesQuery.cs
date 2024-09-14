using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SaveEnergy.Domain;

namespace SaveEnergy.Adapters.Outbound;

public class RepositoriesQuery(
    ILogger<RepositoriesQuery> logger,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ICanAuthenticate authenticator) : IRepositoriesQuery
{
    public async Task<IEnumerable<Repository>> Execute()
    {
        var accessTokenResponse = await authenticator.RequestAccessToken();
        
        // Get the 1 repository which has been pushed to most recently.
        // GET https://api.github.com/user/repos?affiliation=owner&sort=pushed&direction=desc&per_page=1&page=1
        // Accept: application/json
        // Authorization: Bearer {{access_token}}
        var repositoryReadingClient = httpClientFactory.CreateClient();
        repositoryReadingClient.DefaultRequestHeaders.Add("Accept", "application/json");
        repositoryReadingClient.DefaultRequestHeaders.Add("User-Agent", "SaveEnergy");
        repositoryReadingClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessTokenResponse.Token);
        repositoryReadingClient.BaseAddress =
            new Uri(configuration["GitHub:ApiBaseAddress"] ?? "https://api.github.com");
        logger.LogDebug("API base address: {ApiBaseAddress}", repositoryReadingClient.BaseAddress);

        using var undecodedRepositoriesResponse =
            await repositoryReadingClient.GetAsync(
                "/user/repos?affiliation=owner&sort=pushed&direction=desc&per_page=100&page=1");

        logger.LogDebug("Final request URI: {RequestUri}", undecodedRepositoriesResponse.RequestMessage.RequestUri);

        undecodedRepositoriesResponse.EnsureSuccessStatusCode();

        // TODO: Handle exceptions, e.g. HttpRequestException
        // TODO: Consider paging when processing the GitHub API response

        var repositories = await undecodedRepositoriesResponse.Content.ReadFromJsonAsync<IEnumerable<Repository>>();
        return repositories ?? [];
    }
}