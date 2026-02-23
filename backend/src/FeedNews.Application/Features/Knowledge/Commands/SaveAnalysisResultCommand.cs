using FeedNews.Domain.Entities;
using MediatR;

namespace FeedNews.Application.Features.Knowledge.Commands;

/// <summary>
/// Command to save analysis result to database.
/// </summary>
public record SaveAnalysisResultCommand(
    ArticleAnalysisResult AnalysisResult
) : IRequest<bool>;
