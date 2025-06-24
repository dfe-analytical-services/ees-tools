using System.Text;
using Cocona;
using SearchRankingTool.Utils;

var app = CoconaApp.Create();

app.AddCommand("search", async (
        [Argument("azureUrl", Description = "Azure search url")]string azureUrl,
        [Argument("apikey", Description = "Azure search url")]string apikey,
        [Argument("searchType", Description = "Search type")]SearchType searchType,
        [Argument("query", Description = "The search query to run. Enclose in quotes.")]string searchQuery, 
        [Argument("urlToRank", Description = "The url to rank. Include https://")]string urlToRank)
    =>
{
    var runner = new Runner(azureUrl, apikey, searchType);
    await runner.RunSingleQuery(
        searchQuery,
        urlToRank
    );
}).WithDescription("Run a single search query and output the ranking of a specified URL.");

app.AddCommand("csv", async (
    [Argument("azureUrl", Description = "Azure search url")]string azureUrl,
    [Argument("apikey", Description = "Azure search url")]string apikey,
    [Argument("searchType", Description = "Search type")]SearchType searchType,
    [Argument("input", Description = "The filename of the csv file to process")]string inputFilename,
    [Option(name:"output", Description = "The results will be written to this filename. If not specified, an output name will be generated automatically.")]string? outputFilename) =>
{
    var sb = new StringBuilder();
    var runner = new Runner(azureUrl, apikey, searchType, s => sb.AppendLine(s));
    await runner.RunFile(inputFilename);
    
    // Save to results file
    outputFilename ??= GenerateOutputFilename(inputFilename);
    File.WriteAllText(outputFilename, sb.ToString());
    
    Console.WriteLine($"Results have been written to {outputFilename}");

    string GenerateOutputFilename(string filename)
    {
        var name = Path.GetFileNameWithoutExtension(filename);
        var directory = Path.GetDirectoryName(filename) ?? string.Empty;
        var extension = Path.GetExtension(filename);
        return Path.Combine(directory, $"{name}-result{extension}");
    }
}).WithDescription("Process a csv file that contains a search query followed by a comma, and then the expected top ranking url. Results will be written to an output file.");

app.Run();
