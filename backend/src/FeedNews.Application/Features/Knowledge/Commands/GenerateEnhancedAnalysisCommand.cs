using FeedNews.Domain.Entities;
using MediatR;

namespace FeedNews.Application.Features.Knowledge.Commands;

/// <summary>
/// Command to generate enhanced analysis for an article using knowledge base and questions.
/// </summary>
public record GenerateEnhancedAnalysisCommand(
    Guid NewsId,
    string Category,
    string OriginalSummary,
    List<KnowledgeEntry> RelevantKnowledge,
    List<AnalysisQuestion> Questions
) : IRequest<ArticleAnalysisResult>;
