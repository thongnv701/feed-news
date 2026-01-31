namespace FeedNews.Domain.Extensions;

public class AuthExtensions
{
    public static string RedisVerfiyCodeKey = "VerificationLoginCode-{0}";

    public static string GetRedisVerifyCodeKey(string email)
    {
        return string.Format(RedisVerfiyCodeKey, email);
    }
}