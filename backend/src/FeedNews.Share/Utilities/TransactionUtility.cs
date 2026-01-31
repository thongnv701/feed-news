using System.Transactions;

namespace FeedNews.Share.Utilities;

public static class TransactionScopeBuilder
{
    public static TransactionScope CreateTransactionScope()
    {
        return new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = TransactionManager.DefaultTimeout
            },
            TransactionScopeAsyncFlowOption.Enabled
        );
    }
}