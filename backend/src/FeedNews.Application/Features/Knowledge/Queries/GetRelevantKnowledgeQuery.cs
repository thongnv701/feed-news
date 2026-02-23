using FeedNews.Domain.Entities;
using MediatR;

namespace FeedNews.Application.Features.Knowledge.Queries;

/// <summary>
/// Query to retrieve relevant knowledge entries based on article summary and category.
/// </summary>
public record GetRelevantKnowledgeQuery(
    string Category,
    string ArticleSummary,
    int MaxResults = 5
) : IRequest<List<KnowledgeEntry>>;
