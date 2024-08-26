using System.Text.Json.Serialization;

namespace SaveEnergy;

public readonly record struct DeviceCodeResponse
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
