using FeedNews.Application.Contracts.Repositories;

namespace FeedNews.Application.Common.Repositories;

public interface IUnitOfWork
{
    bool IsTransaction { get; }

    INewsRepository News { get; }
    IKnowledgeRepository Knowledge { get; }
    IAnalysisQuestionRepository AnalysisQuestions { get; }
    IKnowledgeDisputeRepository KnowledgeDisputes { get; }
    IArticleAnalysisResultRepository AnalysisResults { get; }

    Task BeginTransactionAsync();

    Task CommitTransactionAsync();

    Task SaveChangesAsync();

    void RollbackTransaction();
}
