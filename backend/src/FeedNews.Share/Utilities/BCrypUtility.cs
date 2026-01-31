namespace FeedNews.Share.Utilities;

public class BCrypUtility
{
    public static string Hash(string input)
    {
        return BCrypt.Net.BCrypt.HashPassword(input);
    }

    public static bool Verify(string input, string hashedValue)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(input, hashedValue);
        }
        catch
        {
            return false;
        }
    }
}