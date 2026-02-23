using FeedNews.Application.Common.Repositories;
using FeedNews.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FeedNews.Application.Features.Knowledge.Commands;

public class SaveAnalysisResultCommandHandler : IRequestHandler<SaveAnalysisResultCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SaveAnalysisResultCommandHandler> _logger;

    public SaveAnalysisResultCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<SaveAnalysisResultCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(SaveAnalysisResultCommand request, CancellationToken cancellationToken)
    {
        if (request.AnalysisResult == null)
        {
            _logger.LogWarning("Attempted to save null analysis result");
            return false;
        }

        try
        {
            _logger.LogInformation("Saving analysis result for NewsId: {NewsId}", request.AnalysisResult.NewsId);

            // Validate that the NewsId exists in the news_feeds table (CRITICAL: FK constraint)
            var newsExists = await _unitOfWork.News.ExistsAsync(request.AnalysisResult.NewsId);
            if (!newsExists)
            {
                _logger.LogError("Cannot save analysis result: NewsId {NewsId} does not exist in news_feeds table", request.AnalysisResult.NewsId);
                throw new InvalidOperationException($"News record with ID {request.AnalysisResult.NewsId} does not exist. Cannot save analysis result due to foreign key constraint.");
            }

            await _unitOfWork.AnalysisResults.AddAsync(request.AnalysisResult);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully saved analysis result with Id: {Id}", request.AnalysisResult.Id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving analysis result for NewsId: {NewsId}", request.AnalysisResult.NewsId);
            throw;
        }
    }
}
