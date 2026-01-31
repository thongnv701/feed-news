using FeedNews.Application.Contracts.Repositories;

namespace FeedNews.Application.Common.Repositories;

public interface IUnitOfWork
{
    bool IsTransaction { get; }

    INewsRepository News { get; }

    Task BeginTransactionAsync();

    Task CommitTransactionAsync();

    Task SaveChangesAsync();

    void RollbackTransaction();
}