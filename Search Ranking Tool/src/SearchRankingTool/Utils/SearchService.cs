using FuzzySharp;
using QuickType;
using SearchRankingTool.Extensions;

namespace SearchRankingTool.Utils;

public class SearchService(string url, string apikey, Action<string> output)
{
    private AzureSearchHttpClient BuildSearchClient()
    {
        // Create a client
        return new AzureSearchHttpClient(new Uri(url), apikey);
    }

    public async Task<int> Search(string searchText, string expectedUri)
    {
        var client = BuildSearchClient();

        var response = await client.SearchAsync<Welcome>(searchText);

        int rank = response.Values
            .WithIndex()
            .Where(indexedSearchResult => IsMatch(indexedSearchResult.Item, expectedUri))
            .Select<(Value Item, int Index), int?>(x => x.Index + 1 /* one based */)
            .FirstOrDefault()
            ?? 11; // default to 11 if not found

        
         // OUTPUT - Not found information
         if (rank == 11)
               OutputClosestMatch(searchText, expectedUri, response.Values);
        
        return rank;
    }

    private bool IsMatch(Value searchResult, string expectedUri)
    {
        var actualUrl = BuildUrlFromSearchResult(searchResult);
        return
            actualUrl.Equals(expectedUri, StringComparison.InvariantCultureIgnoreCase)
            || expectedUri.StartsWith(actualUrl, StringComparison.InvariantCultureIgnoreCase);
    }

    private string BuildUrlFromSearchResult(Value searchResult) => $"https://explore-education-statistics.service.gov.uk/find-statistics/{searchResult.PublicationSlug}";
    
    private void OutputClosestMatch(string searchText, string expectedUri, Value[] responseValues)
    {
        var closestMatch = responseValues
            .WithIndex()
            .OrderByDescending(x => Fuzz.Ratio(expectedUri, BuildUrlFromSearchResult(x.Item)))
            .First();

        output($"Search for {searchText} not found.");
        output($"Closest fuzzy match: Rank {closestMatch.Index+1}:");
        output($"Expected   : {expectedUri}");
        output($"Closest    : {BuildUrlFromSearchResult(closestMatch.Item)}");
        output($"#1 Top Ranked for {searchText}: {BuildUrlFromSearchResult(responseValues.First())}");
        output(string.Empty);
    }
}