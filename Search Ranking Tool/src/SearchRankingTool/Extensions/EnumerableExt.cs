namespace SearchRankingTool.Extensions;

public static class EnumerableExt
{
    public static IEnumerable<(T Item, int Index)> WithIndex<T>(this IEnumerable<T> source) => source.Select((item, index) => (item, index));
}