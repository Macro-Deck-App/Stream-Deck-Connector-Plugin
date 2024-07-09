using System.Text.Json.Serialization;

namespace MacroDeck.StreamDeckConnectorPlugin.GitHub;

public class GitHubAsset
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("browser_download_url")]
    public string? DownloadUrl { get; set; }

    [JsonPropertyName("size")]
    public long? Size { get; set; }
}