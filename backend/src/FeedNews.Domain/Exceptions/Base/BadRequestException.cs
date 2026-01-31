namespace FeedNews.Domain.Exceptions.Base;

public class BadRequestException : Exception
{
    protected BadRequestException(string message)
        : base(message)
    {
    }
}