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
    var runner = new Runner(new Uri(azureUrl), apikey, searchType);
    await runner.RunSingleQuery(
        searchQuery,
        urlToRank
    );
}).WithDescription("Run a single search query and output the ranking of a specified URL.");

app.AddCommand("csv", async (
    [Argument("azureUrl", Description = "Azure search url")]string azureUrl,
    [Argument("apikey", Description = "Azure search url")]string apikey,
    [Argument("searchType", Description = "Search type")]SearchType searchType,
    [Argument("input", Description = "The filename of the csv file to process")]string inputFilename) =>
{
    var csvOutput = new StringBuilder();
    var reportOutput = new StringBuilder();
    var runner = new Runner(new Uri(azureUrl), apikey, searchType, s => csvOutput.AppendLine(s), s => reportOutput.AppendLine(s));
    try
    {
        await runner.RunFile(inputFilename);
    }
    catch (FileNotFoundException e)
    {
        Console.WriteLine($"Error: input csv file was not found: {e.FileName}");
        return -1;
    }
    catch (Exception e)
    {
        Console.WriteLine($"Error: {e.Message}");
        return -1;
    }
    
    // Save to results file
    var csvOutputFilename = GenerateOutputFilename("results", "csv");
    File.WriteAllText(csvOutputFilename, csvOutput.ToString());

    var reportOutputFilename = GenerateOutputFilename("report", "txt");
    File.WriteAllText(reportOutputFilename, reportOutput.ToString());
    
    Console.WriteLine($"Results have been written to {csvOutputFilename}");
    Console.WriteLine($"Report has been written to {reportOutputFilename}");
    return 0;

    string GenerateOutputFilename(string fileType, string extension)
    {
        var name = Path.GetFileNameWithoutExtension(inputFilename);
        var directory = Path.GetDirectoryName(inputFilename) ?? string.Empty;
        return Path.Combine(directory, $"{name}-{searchType.ToString()}-{fileType}.{extension}");
    }
}).WithDescription("Process a csv file that contains a search query followed by a comma, and then the expected top ranking url. Results will be written to an output file.");

app.Run();
