using QuickType;
using SearchRankingTool.Extensions;

namespace SearchRankingTool.Utils;

internal class SearchService(Uri url, string apikey, SearchType searchType, Action<string> output)
{
    private AzureSearchHttpClient BuildSearchClient()
    {
        // Create a client
        return new AzureSearchHttpClient(url, apikey);
    }

    public async Task<int> Search(string searchText, string expectedUri)
    {
        var client = BuildSearchClient();

        var response = await client.SearchAsync<Welcome>(searchText, searchType);

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
        output($"Search for {searchText} not found.");
        foreach (var responseValue in responseValues.WithIndex())
        {
            output($"{responseValue.Index + 1}. {responseValue.Item.PublicationSlug}");
        }
        output($"Expected   : {expectedUri}");
        output(string.Empty);
    }
}