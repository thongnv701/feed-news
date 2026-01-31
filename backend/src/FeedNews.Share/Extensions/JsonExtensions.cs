using System.Text.Json;
using System.Text.Json.Serialization;

namespace FeedNews.Share.Extensions;

public static class JsonExtensions
{
    private static readonly JsonSerializerOptions? _defaultSerializerSettings = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    public static string ToJson(this object obj)
    {
        return ReferenceEquals(obj, null) ? string.Empty : JsonSerializer.Serialize(obj, _defaultSerializerSettings);
    }

    public static T? FromJson<T>(this string json)
    {
        return string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json, _defaultSerializerSettings);
    }

    public static T? DeepCopyJson<T>(this T input)
    {
        if (ReferenceEquals(input, null)) return default;
        string jsonString = JsonSerializer.Serialize(input);
        return JsonSerializer.Deserialize<T>(jsonString);
    }
}