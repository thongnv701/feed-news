using FeedNews.Application.Shared;

namespace FeedNews.Share.Model.Response;

public class Result
{
    protected internal Result(bool isSuccess, bool isWarning, string trackId, Error error)
    {
        if (isSuccess && error != Error.None) throw new InvalidOperationException();

        if (!isSuccess && error == Error.None) throw new InvalidOperationException();

        if (isSuccess && isWarning) throw new InvalidOperationException();

        IsSuccess = isSuccess;
        IsWarning = isWarning;
        Error = error;
        TrackId = trackId;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess && !IsWarning;

    public bool IsWarning { get; }
    public string TrackId { get; set; }

    public Error Error { get; }

    public static Result Success(string trackId)
    {
        return new Result(true, false, trackId, Error.None);
    }

    public static Result<TValue> Success<TValue>(TValue value, string trackId)
    {
        return new Result<TValue>(value, true, false, trackId, Error.None);
    }

    public static Result Failure(string trackId, Error error)
    {
        return new Result(false, false, trackId, error);
    }

    public static Result<TValue> Failure<TValue>(Error error, string trackId)
    {
        return new Result<TValue>(default, false, false, trackId, error);
    }

    public static Result<TValue> Failure<TValue>(TValue value, Error error, string trackId)
    {
        return new Result<TValue>(value, false, false, trackId, error);
    }

    public static Result<TValue> Create<TValue>(TValue? value, string trackId)
    {
        return value is not null ? Success(value, trackId) : Failure<TValue>(Error.NullValue, trackId);
    }
}