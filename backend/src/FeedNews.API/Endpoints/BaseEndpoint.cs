using FeedNews.Share.Model.Response;

namespace FeedNews.API.Endpoints;

public class BaseEndpoint
{
    protected IResult HandleResult<T>(Result<T> result)

    {
        if (result.IsSuccess && result.Value != null) return TypedResults.Ok(result);

        if (result.IsSuccess && result.Value == null) return TypedResults.NoContent();

        return TypedResults.BadRequest(result);
    }
}