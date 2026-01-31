namespace FeedNews.Share.Utilities;

public static class Utility
{
    public static string GetPrefixSlowLyLog(long mSec, long sMin = 0)
    {
        string slowLyWarn = string.Empty;
        if (sMin == 0) sMin = 1000;

        if (mSec < sMin) return slowLyWarn;
        long responseSecond = mSec / 1000;
        slowLyWarn = responseSecond switch
        {
            >= 50 => " slowly50 ",
            >= 40 => " slowly40 ",
            >= 30 => " slowly30 ",
            >= 20 => " slowly20 ",
            >= 10 => " slowly10 ",
            >= 5 => " slowly05 ",
            >= 1 => " slowly01 ",
            _ => " slowly "
        };
        return slowLyWarn;
    }

    public static string ConvertMillisecondToHourMinSec(long mSec)
    {
        TimeSpan t = TimeSpan.FromMilliseconds(mSec);
        List<string> lst = new();
        if (t.Hours > 0) lst.Add($"{t.Hours:D2}h");

        if (t.Minutes > 0) lst.Add($"{t.Minutes:D2}m");

        if (t.Seconds > 0) lst.Add($"{t.Seconds:D2}s");

        lst.Add($"{t.Milliseconds:D2}ms");
        string result = string.Join(":", lst);
        return result;
    }

    /*public static string GetQueryInjectParam(this string query, DynamicParameters parameters)
    {
        try
        {
            var sql = query;

            foreach (var parameterName in parameters.ParameterNames)
            {
                var parameterValue = parameters.Get<object>(parameterName);
                var formattedValue = parameterValue switch
                {
                    null => "null",
                    string or DateTime => $"'{parameterValue}'",
                    DbString dbString => $"'{dbString.Value}'",
                    IEnumerable list =>
                        $"array[{string.Join(",", list.Cast<object>().Select(e => e is string ? $"'{e}'" : e?.ToString()))}]",
                    _ => parameterValue.ToString()
                };
                sql = sql.Replace($"@{parameterName}", formattedValue);
            }

            var demo = Regex.Replace(sql, @"\r\n?|\n|\t", " ");

            const string reduceMultiSpace = "[ ]{2,}";

            var line = Regex.Replace(demo.Replace("\t", " "), reduceMultiSpace, " ");

            return line;
        }
        catch (Exception e)
        {
            Console.WriteLine($"GetQueryInjectParam with Exception: {e.Message}, StackTrace: {e.StackTrace}");
            return query;
        }
    }*/
}