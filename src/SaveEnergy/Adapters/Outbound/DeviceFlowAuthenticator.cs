using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SaveEnergy.Domain;

namespace SaveEnergy.Adapters.Outbound;

internal class DeviceFlowAuthenticator : ICanAuthenticate
{
    // Using a hard-coded URI makes the program easier to use.
    //
    // Without a default URI, the program would show an error and stop if the
    // user hasn't set one. The user would then need to set the GitHub URL and
    // restart the program.
#pragma warning disable S1075
    private const string DefaultAuthenticationBaseAddress = "https://github.com";
#pragma warning restore S1075

    private readonly HttpClient _authenticationClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DeviceFlowAuthenticator> _logger;

    public DeviceFlowAuthenticator(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<DeviceFlowAuthenticator> logger
    )
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;

        _authenticationClient = CreateAuthenticationClient();
    }

    private HttpClient CreateAuthenticationClient()
    {
        var authenticationClient = _httpClientFactory.CreateClient();

        authenticationClient.BaseAddress = new Uri(
            _configuration["GitHub:AuthenticationBaseAddress"] ?? DefaultAuthenticationBaseAddress
        );
        
        authenticationClient.DefaultRequestHeaders.Add("Accept", "application/json");

        _logger.LogDebug(
            "Authentication base address: {AuthenticationBaseAddress}",
            authenticationClient.BaseAddress
        );

        return authenticationClient;
    }

    public async Task<AccessToken> RequestAccessToken()
    {
        var deviceCodeResponse = await RequestDeviceCode();

        Console.WriteLine(
            "Please visit {0} and enter the code \"{1}\" to authenticate this application.",
            deviceCodeResponse.VerificationUri,
            deviceCodeResponse.UserCode
        );

        var accessTokenResponse = await WaitUntilAccessGranted(deviceCodeResponse);

        return new AccessToken(accessTokenResponse.AccessToken);
    }

    private async Task<DeviceCodeResponse> RequestDeviceCode()
    {
        var deviceCodeResponse = await PostJsonAsync<DeviceCodeResponse>(
            "/login/device/code",
            new { client_id = _configuration["GitHub:ClientId"] }
        );

        _logger.LogDebug("Device code: {DeviceCodeResponse}", deviceCodeResponse);

        return deviceCodeResponse;
    }

    private async Task<TResult> PostJsonAsync<TResult>(string requestUri, object value)
    {
        using var httpResponse = await _authenticationClient.PostAsJsonAsync(requestUri, value);

        httpResponse.EnsureSuccessStatusCode();

        return await httpResponse.Content.ReadFromJsonAsync<TResult>();
    }

    private async Task<AccessTokenResponse> WaitUntilAccessGranted(
        DeviceCodeResponse deviceCodeResponse
    )
    {
        var secondsPassed = Stopwatch.StartNew();

        var accessTokenResponse = new AccessTokenResponse();
        while (
            secondsPassed.Elapsed.TotalSeconds < deviceCodeResponse.ExpiresIn
            && string.IsNullOrEmpty(accessTokenResponse.AccessToken)
        )
        {
            Thread.Sleep(deviceCodeResponse.Interval * 1000);
            _logger.LogDebug(
                "Checking for authentication success for another {0} minutes ...",
                (deviceCodeResponse.ExpiresIn - secondsPassed.Elapsed.TotalSeconds) / 60
            );

            accessTokenResponse = await RequestAccessToken(deviceCodeResponse);
        }

        secondsPassed.Stop();
        return accessTokenResponse;
    }

    private async Task<AccessTokenResponse> RequestAccessToken(
        DeviceCodeResponse deviceCodeResponse
    )
    {
        var accessTokenResponse = await PostJsonAsync<AccessTokenResponse>(
            "/login/oauth/access_token",
            new
            {
                client_id = _configuration["GitHub:ClientId"],
                device_code = deviceCodeResponse.DeviceCode,
                grant_type = "urn:ietf:params:oauth:grant-type:device_code",
            }
        );

        _logger.LogDebug("Access token: {AccessTokenResponse}", accessTokenResponse);

        return accessTokenResponse;
    }

    private readonly record struct DeviceCodeResponse
    {
        [JsonPropertyName("device_code")]
        public string DeviceCode { get; init; }

        [JsonPropertyName("user_code")]
        public string UserCode { get; init; }

        [JsonPropertyName("verification_uri")]
        public string VerificationUri { get; init; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; init; }

        [JsonPropertyName("interval")]
        public int Interval { get; init; }
    }

    private readonly record struct AccessTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; init; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; init; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; init; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; init; }

        [JsonPropertyName("scope")]
        public string Scope { get; init; }
    }
}
