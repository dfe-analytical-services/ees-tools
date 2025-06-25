using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using SearchRankingTool.Extensions;

namespace SearchRankingTool.Utils;

internal class AzureSearchHttpClient(Uri searchServiceUri, string apiKey)
{
    public async Task<T> SearchAsync<T>(string searchText, SearchType searchType)
    {
        searchText = ModifySearchText(searchText, searchType);

        var httpClient = new HttpClient();
        
        // Json Body to Post
        var body = BuildPayload(searchText, searchType);

        // Append API version to URL
        // 2025-05-01-preview -- support for spellcheck
        var searchServiceUriWithApiVersion = QueryHelpers.AddQueryString(searchServiceUri.OriginalString, "api-version", "2025-05-01-preview");;

        var message = new HttpRequestMessage(HttpMethod.Post, new Uri(searchServiceUriWithApiVersion))
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        message.Headers.Add("api-key", apiKey);

        var httpResponseMessage = await httpClient.SendAsync(message);
        httpResponseMessage.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<T>(await httpResponseMessage.Content.ReadAsStringAsync()) ?? throw new Exception("Could not deserialize response");
    }

    private string BuildPayload(string searchText, SearchType searchType)
    {
        var isSemanticSearch = searchType.ToString().StartsWith("Semantic");
        var queryType = isSemanticSearch ? "semantic" : "full";
        var optionalSpellCheck = searchType == SearchType.SemanticSpellChecked 
            ? """
                "speller": "lexicon",
                "queryLanguage": "en-us",
              """ 
            : string.Empty;
        
        var optionalSemanticConfiguration = isSemanticSearch
            ? """
                 "semanticConfiguration": "semantic-configuration-1",
              """
            : string.Empty;
        
        var payload = $$$"""
               {
                 "search": "{{{searchText}}}",
                 "queryType": "{{{queryType}}}",
                 {{{optionalSpellCheck}}}
                 "facets": ["themeId,count:60,sort:count", "releaseType"],
                 "highlight": "content",
                 "scoringProfile": "scoring-profile-1",
                 "searchMode": "any",
                 "select": "content,releaseSlug,releaseType,releaseVersionId,publicationSlug,published,summary,themeTitle,title",
                 "top": 10,
                 {{{optionalSemanticConfiguration}}}
                 "count": true
               }
               """;
        return payload;
    }
    
    private string ModifySearchText(string s, SearchType searchType)
    {
        return searchType switch
        {
            SearchType.Semantic => s,
            SearchType.SemanticSpellChecked => s,
            SearchType.FullText => s,
            SearchType.FullTextFuzzy2 => Fuzzy2(s),
            SearchType.FullTextFuzzy3 => Fuzzy3(s),
            SearchType.FullTextFuzzy2Wildcard => FuzzyAndWildcard(s),
            SearchType.SemanticScoringProfile => s,
            _ => throw new ArgumentOutOfRangeException(nameof(searchType), searchType, null)
        };
        
        // term =>  termA~ OR termB~ OR... 
        string Fuzzy2(string words) => 
            words
                .Split(" ")
                .Select(word => word.Trim())
                .Select(word => word.Length > 3 
                    ? $"{word}~" 
                    : word)
                .Join(" OR ");
        
        // term =>  termA~3 OR termB~3 OR...
        string Fuzzy3(string words) => 
            words
                .Split(" ")
                .Select(word => word.Trim())
                .Select(word => word.Length > 3 
                    ? $"{word}~3" 
            // $"{word}* OR {word}~" 
                    : word)
                .Join(" OR ");
        
        // term =>  termA* OR termA~ OR termB* OR termB~ OR... 
        string FuzzyAndWildcard(string words) => 
            words
                .Split(" ")
                .Select(word => word.Trim())
                .Select(word => word.Length > 3 
                    ? $"{word}* OR {word}~" 
                    : word)
                .Join(" OR ");
    }
}