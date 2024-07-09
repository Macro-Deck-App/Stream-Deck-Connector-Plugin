using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace MacroDeck.StreamDeckConnectorPlugin.GitHub;

public static class GitHubService
{
    private const string GitHubApi = "https://api.github.com/repos/Macro-Deck-App/Macro-Deck-Stream-Deck-Connector/";

    public static async Task<GitHubRelease?> GetLatestRelease()
    {
        try
        {
            using var httpClient = GetHttpClient();
            return await httpClient.GetFromJsonAsync<GitHubRelease>("releases/latest");
        }
        catch
        {
            return null;
        }
    }

    private static HttpClient GetHttpClient()
    {
        var httpClient = new HttpClient();;
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Macro Deck StreamDeckConnector");
        httpClient.BaseAddress = new Uri(GitHubApi);
        return httpClient;
    }
}