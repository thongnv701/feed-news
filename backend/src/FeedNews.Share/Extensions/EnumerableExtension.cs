namespace FeedNews.Share.Extensions;

/// <summary>
///     Provides extension methods for enumerable operations.
/// </summary>
public static class EnumerableExtension
{
    /// <summary>
    ///     Determines whether the specified list is null or an empty list.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to check.</param>
    /// <returns>true if the list is null or empty; otherwise, false.</returns>
    public static bool IsListNullOrEmpty<T>(this IEnumerable<T>? list)
    {
        return list == null || !list.Any();
    }

    /// <summary>
    ///     Determines whether the specified list is not null and not an empty list.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to check.</param>
    /// <returns>true if the list is not null and not empty; otherwise, false.</returns>
    public static bool IsNotEmpty<T>(this IEnumerable<T>? list)
    {
        return list != null && list.Any();
    }

    /// <summary>
    ///     Tries to parse each string in the specified list to an integer.
    /// </summary>
    /// <param name="stringList">The list of strings to parse.</param>
    /// <returns>An enumerable of integers that were successfully parsed from the input list.</returns>
    public static IEnumerable<int> TryParseInt(this IEnumerable<string> stringList)
    {
        List<int> intList = new();

        foreach (string str in stringList)
            if (int.TryParse(str, out int result))
                intList.Add(result);

        return intList;
    }
}