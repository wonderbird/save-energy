using System.Text.Json.Serialization;

namespace SaveEnergy.Domain;

public readonly record struct Repository(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("html_url")] string HtmlUrl
);
