using FeedNews.Application.Common.Repositories;
using FeedNews.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FeedNews.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private const string ErrorNotOpenTransaction = "You not open transaction yet!";
    private const string ErrorAlreadyOpenTransaction = "Transaction already open";

    public UnitOfWork(FeedNewsContext context)
    {
        this.Context = context;
    }

    internal FeedNewsContext Context { get; }

    public bool IsTransaction { get; private set; }

    public Task BeginTransactionAsync()
    {
        if (IsTransaction) throw new Exception(ErrorAlreadyOpenTransaction);

        IsTransaction = true;
        return Task.CompletedTask;
    }

    public async Task CommitTransactionAsync()
    {
        if (!IsTransaction) throw new Exception(ErrorNotOpenTransaction);

        await Context.SaveChangeAsync().ConfigureAwait(false);
        IsTransaction = false;
    }

    public void RollbackTransaction()
    {
        if (!IsTransaction) throw new Exception(ErrorNotOpenTransaction);

        IsTransaction = false;

        foreach (EntityEntry entry in Context.ChangeTracker.Entries()) entry.State = EntityState.Detached;
    }
}