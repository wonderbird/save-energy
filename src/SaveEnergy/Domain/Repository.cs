using System.Text.Json.Serialization;

namespace SaveEnergy.Domain;

public readonly record struct Repository(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("pushed_at")] DateTime PushedAt,
    [property: JsonPropertyName("html_url")] string HtmlUrl,
    [property: JsonPropertyName("ssh_url")] string SshUrl,
    [property: JsonPropertyName("clone_url")] string CloneUrl
    
);
