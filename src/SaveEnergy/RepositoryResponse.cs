using System.Text.Json.Serialization;

namespace SaveEnergy;

internal readonly record struct RepositoryResponse
{
    [JsonPropertyName("name")]
    public string Name { get; init; }
    
    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; init; }
}
