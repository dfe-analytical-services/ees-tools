namespace SearchRankingTool.Extensions;

public static class StringExtensions
{
    public static string Join(this IEnumerable<string> source, string separator) => string.Join(separator, source);
}