using QuickType;
using SearchRankingTool.Extensions;

namespace SearchRankingTool.Utils;

internal class SearchService(
    Uri url,
    string apikey,
    SearchType searchType,
    Action<string> reportOutput)
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

        int oneBasedRank = response.Values
            .OrderByDescending(result => result.SearchRerankerScore)
            .WithIndex()
            .Where(indexedSearchResult => IsMatch(indexedSearchResult.Item, expectedUri))
            .Select<(Value Item, int Index), int?>(x => x.Index + 1 /* one based */)
            .FirstOrDefault()
            ?? 11; // default to 11 if not found

        
         // OUTPUT - Not found information
         if (oneBasedRank > 1)
               OutputHigherMatches(searchText, expectedUri, response.Values, oneBasedRank);
        
        return oneBasedRank;
    }

    private bool IsMatch(Value searchResult, string expectedUri)
    {
        var actualUrl = BuildUrlFromSearchResult(searchResult);
        return
            actualUrl.Equals(expectedUri, StringComparison.InvariantCultureIgnoreCase)
            || expectedUri.StartsWith(actualUrl, StringComparison.InvariantCultureIgnoreCase);
    }

    private string BuildUrlFromSearchResult(Value searchResult) => $"https://explore-education-statistics.service.gov.uk/find-statistics/{searchResult.PublicationSlug}";
    
    private void OutputHigherMatches(string searchText, string expectedUri, Value[] responseValues, int oneBasedRank)
    {
        reportOutput($"Search for \"{searchText}\" ranked {oneBasedRank}.");
        foreach (var responseValue in responseValues.WithIndex())
        {
            if (responseValue.Index + 1 == oneBasedRank) break; // Report up to the result we were looking for.
            reportOutput($"{responseValue.Index + 1}. {responseValue.Item.PublicationSlug}");
        }
        reportOutput($"Expected   : {expectedUri}");
        reportOutput(string.Empty);
    }
}