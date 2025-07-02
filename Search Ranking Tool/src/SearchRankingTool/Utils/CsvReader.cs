namespace SearchRankingTool.Utils;

internal static class CsvReader
{
    public static IEnumerable<string[]> GetCsvDataFromFile(string filename)
    {
        var fileInfo = new FileInfo(filename);
        if (!fileInfo.Exists) throw new FileNotFoundException($"File {filename} not found", fileInfo.FullName);

        using var streamReader = fileInfo.OpenText();
        while(true)
        {
            var rawText = streamReader.ReadLine();
            if (rawText == null) yield break;
            yield return rawText.Split(',');
        }
    }
}