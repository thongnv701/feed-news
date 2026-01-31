using FeedNews.Application.Common.Abstractions.Messaging;
using FeedNews.Share.Model.Response;

namespace FeedNews.Application.UseCases.Test.Commands.TestValidateError;

public class TestValidateErrorHandler : ICommandHandler<TestValidateErrorCommand, Result>
{
    public Task<Result<Result>> Handle(TestValidateErrorCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}