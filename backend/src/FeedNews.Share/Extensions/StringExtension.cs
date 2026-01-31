using System.Diagnostics.CodeAnalysis;

namespace FeedNews.Share.Extensions;

/// <summary>
///     Provides extension methods for string operations.
/// </summary>
public static class StringExtension
{
    /// <summary>
    ///     Determines whether the specified string is null or an empty string.
    /// </summary>
    /// <param name="input">The string to check.</param>
    /// <returns>true if the input string is null or empty; otherwise, false.</returns>
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? input)
    {
        return string.IsNullOrEmpty(input);
    }

    /// <summary>
    ///     Determines whether the specified string is null, empty, or consists only of white-space characters.
    /// </summary>
    /// <param name="input">The string to check.</param>
    /// <returns>true if the input string is null, empty, or consists only of white-space characters; otherwise, false.</returns>
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? input)
    {
        return string.IsNullOrWhiteSpace(input);
    }

    /// <summary>
    ///     Determines whether the specified string is not null and not an empty string.
    /// </summary>
    /// <param name="input">The string to check.</param>
    /// <returns>true if the input string is not null and not empty; otherwise, false.</returns>
    public static bool IsNotEmpty([NotNullWhen(true)] this string? input)
    {
        return !string.IsNullOrEmpty(input);
    }
}