using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SaveEnergy;

public class CommandLineInterface : IHostedService
{
    private readonly ILogger<CommandLineInterface> _logger;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public CommandLineInterface(ILogger<CommandLineInterface> logger, IConfiguration configuration,
        IHostApplicationLifetime appLifetime, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _appLifetime = appLifetime;
        _httpClientFactory = httpClientFactory;

        _appLifetime.ApplicationStarted.Register(ProcessCommand);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Process ID {ProcessId}", Environment.ProcessId);
        
        return Task.CompletedTask;
    }

    private async void ProcessCommand()
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        
        var clientId = _configuration["GitHub:ClientId"];
        using var undecodedDeviceCodeResponse = await client.PostAsJsonAsync("https://github.com/login/device/code", new
        {
            client_id = clientId,
        });
        undecodedDeviceCodeResponse.EnsureSuccessStatusCode();
        
        var deviceCodeResponse = await undecodedDeviceCodeResponse.Content.ReadFromJsonAsync<DeviceCodeResponse>();
        _logger.LogDebug("Device code: {DeviceCodeResponse}", deviceCodeResponse);

        Console.WriteLine("Please visit {0} and enter the code {1} to authenticate this application.", deviceCodeResponse.VerificationUri, deviceCodeResponse.UserCode);

        var secondsPassed = Stopwatch.StartNew();
        var accessTokenResponse = new AccessTokenResponse();
        while (secondsPassed.Elapsed.TotalSeconds < deviceCodeResponse.ExpiresIn && string.IsNullOrEmpty(accessTokenResponse.AccessToken))
        {
            Thread.Sleep(deviceCodeResponse.Interval * 1000);
            _logger.LogDebug("Checking for authentication success for another {0} minutes ...", (deviceCodeResponse.ExpiresIn - secondsPassed.Elapsed.TotalSeconds) / 60);
            
            using var undecodedAccessTokenResponse = await client.PostAsJsonAsync("https://github.com/login/oauth/access_token", new
            {
                client_id = clientId,
                device_code = deviceCodeResponse.DeviceCode,
                grant_type = "urn:ietf:params:oauth:grant-type:device_code",
            });
            undecodedAccessTokenResponse.EnsureSuccessStatusCode();

            accessTokenResponse = await undecodedAccessTokenResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();
            _logger.LogDebug("Access token: {AccessTokenResponse}", accessTokenResponse);
        }
        secondsPassed.Stop();
        
        // TODO: Handle exceptions, e.g. HttpRequestException
        
        // Get the 1 repository which has been pushed to most recently.
        // GET https://api.github.com/user/repos?affiliation=owner&sort=pushed&direction=desc&per_page=1&page=1
        // Accept: application/json
        // Authorization: Bearer {{access_token}}
        var tracingClient = _httpClientFactory.CreateClient("TracingClient");
        tracingClient.DefaultRequestHeaders.Add("Accept", "application/json");
        tracingClient.DefaultRequestHeaders.Add("User-Agent", "SaveEnergy");
        tracingClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessTokenResponse.AccessToken);

        using var undecodedRepositoriesResponse = await tracingClient.GetAsync("https://api.github.com/user/repos?affiliation=owner&sort=pushed&direction=desc&per_page=1&page=1");
        
        _logger.LogDebug("Final request URI: {RequestUri}", undecodedRepositoriesResponse.RequestMessage.RequestUri);
        
        undecodedRepositoriesResponse.EnsureSuccessStatusCode();
        
        var repositories = await undecodedRepositoriesResponse.Content.ReadFromJsonAsync<IEnumerable<RepositoryResponse>>();
        foreach (var repository in repositories)
        {
            _logger.LogInformation("Repository: {Repository}", repository);
        }
        
        _appLifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}