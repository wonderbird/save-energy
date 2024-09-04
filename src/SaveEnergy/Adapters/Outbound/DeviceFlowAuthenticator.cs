using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SaveEnergy.Domain;

namespace SaveEnergy.Adapters.Outbound;

internal class DeviceFlowAuthenticator(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<DeviceFlowAuthenticator> logger)
    : ICanAuthenticate
{
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
    
    public async Task<AccessToken> RequestAccessToken()
    {
        var authenticationClient = httpClientFactory.CreateClient();
        authenticationClient.DefaultRequestHeaders.Add("Accept", "application/json");
        authenticationClient.BaseAddress =
            new Uri(configuration["GitHub:AuthenticationBaseAddress"] ?? "https://github.com");
        logger.LogDebug("Authentication base address: {AuthenticationBaseAddress}", authenticationClient.BaseAddress);

        var clientId = configuration["GitHub:ClientId"];
        using var undecodedDeviceCodeResponse = await authenticationClient.PostAsJsonAsync("/login/device/code", new
        {
            client_id = clientId,
        });
        undecodedDeviceCodeResponse.EnsureSuccessStatusCode();

        var deviceCodeResponse = await undecodedDeviceCodeResponse.Content.ReadFromJsonAsync<DeviceCodeResponse>();
        logger.LogDebug("Device code: {DeviceCodeResponse}", deviceCodeResponse);

        Console.WriteLine("Please visit {0} and enter the code \"{1}\" to authenticate this application.",
            deviceCodeResponse.VerificationUri, deviceCodeResponse.UserCode);

        var secondsPassed = Stopwatch.StartNew();
        var accessTokenResponse = new AccessTokenResponse();
        while (secondsPassed.Elapsed.TotalSeconds < deviceCodeResponse.ExpiresIn &&
               string.IsNullOrEmpty(accessTokenResponse.AccessToken))
        {
            Thread.Sleep(deviceCodeResponse.Interval * 1000);
            logger.LogDebug("Checking for authentication success for another {0} minutes ...",
                (deviceCodeResponse.ExpiresIn - secondsPassed.Elapsed.TotalSeconds) / 60);

            using var undecodedAccessTokenResponse = await authenticationClient.PostAsJsonAsync(
                "/login/oauth/access_token", new
                {
                    client_id = clientId,
                    device_code = deviceCodeResponse.DeviceCode,
                    grant_type = "urn:ietf:params:oauth:grant-type:device_code",
                });
            undecodedAccessTokenResponse.EnsureSuccessStatusCode();

            accessTokenResponse = await undecodedAccessTokenResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();
            logger.LogDebug("Access token: {AccessTokenResponse}", accessTokenResponse);
        }

        secondsPassed.Stop();
        return new AccessToken(accessTokenResponse.AccessToken);
    }
}