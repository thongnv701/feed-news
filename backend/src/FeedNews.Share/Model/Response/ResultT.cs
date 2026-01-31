using FeedNews.Application.Shared;

namespace FeedNews.Share.Model.Response;

public class Result<TValue> : Result
{
    protected internal Result(TValue? value, bool isSuccess, bool isWarning, string trackId, Error error)
        : base(isSuccess, isWarning, trackId, error)
    {
        Value = value;
        TrackId = trackId;
    }

    public TValue? Value { get; }

    public static implicit operator Result<TValue>(TValue? value)
    {
        return Create(value, Guid.NewGuid().ToString("N"));
    }
}