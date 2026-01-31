using FeedNews.Application.Common.Enums;
using FeedNews.Domain.Enums;

namespace FeedNews.Application.Common.Exceptions;

public class ApiException : Exception
{
    public ApiException(ResponseCode responseCode)
    {
        ErrorCode = (int)responseCode;
        Error = responseCode.ToString();
        ErrorMessage = responseCode.GetDescription();
    }

    public ApiException(int errorCode, string error, string message)
    {
        ErrorCode = errorCode;
        Error = error;
        ErrorMessage = message;
    }

    public ApiException(ResponseCode responseCode, string message)
    {
        ErrorCode = (int)responseCode;
        Error = responseCode.ToString();
        ErrorMessage = message;
    }

    public int ErrorCode { get; }

    public string Error { get; }

    public string ErrorMessage { get; }
}