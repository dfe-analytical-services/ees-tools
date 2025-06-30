namespace SearchRankingTool.Utils;

internal class Runner(
    Uri url,
    string apikey,
    SearchType searchType,
    Action<string>? output = null)
{
    private readonly Action<string> _output = output ?? Console.WriteLine;

    public async Task RunSingleQuery(string searchQuery, string expectedUrl)
    {
        OutputHeader();
        
        // Run 1 search
        await RunSearch(searchQuery, expectedUrl);
    }

    public async Task RunFile(string filename)
    {
        OutputHeader();
        
        // Parse input CSV file
        var data = CsvReader.GetCsvDataFromFile(filename);

        // Skip header
        foreach (var inputs in data)
        {
            var searchQuery = inputs[0];
            var expectedUrl = inputs[1];
            
            Console.WriteLine($"Running: {searchQuery}...");
            await RunSearch(searchQuery, expectedUrl);
        }
    }

    private void OutputHeader() => _output($"query,expected_url,rank");

    private async Task RunSearch(string searchQuery, string expectedUrl)
    {
        SearchService searchService = new(url, apikey, searchType, _output);

        var rank = await searchService.Search(searchQuery.Trim(), expectedUrl);

        _output($"\"{searchQuery}\",\"{expectedUrl}\",{rank}");
    }
}