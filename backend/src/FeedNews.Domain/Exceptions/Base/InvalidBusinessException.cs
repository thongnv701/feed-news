using System.Net;

namespace FeedNews.Domain.Exceptions.Base;

public class InvalidBusinessException : Exception
{
    // Default constructor
    public InvalidBusinessException()
    {
    }

    // Constructor that accepts a message
    public InvalidBusinessException(string message, HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest)
        : base(message)
    {
        HttpStatusCode = httpStatusCode;
    }

    public InvalidBusinessException(string message, params object[] args)
        : base(message)
    {
        HttpStatusCode = HttpStatusCode.BadRequest;
        Args = args;
    }

    public InvalidBusinessException(string message, HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest,
        params object[] args)
        : base(message)
    {
        HttpStatusCode = httpStatusCode;
        Args = args;
    }

    // Constructor that accepts a message and an inner exception
    public InvalidBusinessException(string message, Exception innerException,
        HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest)
        : base(message, innerException)
    {
        HttpStatusCode = httpStatusCode;
    }

    public HttpStatusCode HttpStatusCode { get; private set; }

    public object[] Args { get; private set; } = Array.Empty<object>();
}