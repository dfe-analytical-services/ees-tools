using System.Text;
using System.Text.Json;

namespace SearchRankingTool.Utils;

internal class AzureSearchHttpClient(Uri searchServiceUri, string apiKey)
{
    public async Task<T> SearchAsync<T>(string searchText)
    {
        var httpClient = new HttpClient();
        
        // Json Body to Post
        var body = $$$"""
                      {
                        "count": true,
                        "facets": ["themeId,count:60,sort:count", "releaseType"],
                        "highlight": "content",
                        "queryType": "semantic",
                        "scoringProfile": "scoring-profile-1",
                        "search": "{{{searchText}}}",
                        "searchMode": "any",
                        "select": "content,releaseSlug,releaseType,releaseVersionId,publicationSlug,published,summary,themeTitle,title",
                        "skip": 0,
                        "top": 10,
                        "semanticConfiguration": "semantic-configuration-1"
                      }
                      """;

        var message = new HttpRequestMessage(HttpMethod.Post, searchServiceUri)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        message.Headers.Add("api-key", apiKey);

        var httpResponseMessage = await httpClient.SendAsync(message);
        httpResponseMessage.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<T>(await httpResponseMessage.Content.ReadAsStringAsync()) ?? throw new Exception("Could not deserialize response");
    }
}