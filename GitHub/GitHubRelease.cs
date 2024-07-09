using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MacroDeck.StreamDeckConnectorPlugin.GitHub;

public class GitHubRelease
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("tag_name")]
    public string? TagName { get; set; }

    [JsonPropertyName("assets")]
    public List<GitHubAsset>? Assets { get; set; }
}