using FeedNews.Application.Common.Repositories;
using FeedNews.Application.Contracts.Repositories;
using FeedNews.Infrastructure.Persistence.Contexts;
using FeedNews.Infrastructure.Repositories;

namespace FeedNews.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private const string ErrorNotOpenTransaction = "You not open transaction yet!";
    private const string ErrorAlreadyOpenTransaction = "Transaction already open";
    private INewsRepository? _newsRepository;
    private IKnowledgeRepository? _knowledgeRepository;
    private IAnalysisQuestionRepository? _analysisQuestionRepository;
    private IKnowledgeDisputeRepository? _knowledgeDisputeRepository;
    private IArticleAnalysisResultRepository? _analysisResultRepository;

    public UnitOfWork(FeedNewsContext context)
    {
        this.Context = context;
    }

    internal FeedNewsContext Context { get; }

    public bool IsTransaction { get; private set; }

    public INewsRepository News
    {
        get
        {
            _newsRepository ??= new NewsRepository(Context);
            return _newsRepository;
        }
    }

    public IKnowledgeRepository Knowledge
    {
        get
        {
            _knowledgeRepository ??= new KnowledgeRepository(Context);
            return _knowledgeRepository;
        }
    }

    public IAnalysisQuestionRepository AnalysisQuestions
    {
        get
        {
            _analysisQuestionRepository ??= new AnalysisQuestionRepository(Context);
            return _analysisQuestionRepository;
        }
    }

    public IKnowledgeDisputeRepository KnowledgeDisputes
    {
        get
        {
            _knowledgeDisputeRepository ??= new KnowledgeDisputeRepository(Context);
            return _knowledgeDisputeRepository;
        }
    }

    public IArticleAnalysisResultRepository AnalysisResults
    {
        get
        {
            _analysisResultRepository ??= new ArticleAnalysisResultRepository(Context);
            return _analysisResultRepository;
        }
    }

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

    public async Task SaveChangesAsync()
    {
        await Context.SaveChangeAsync().ConfigureAwait(false);
    }

    public void RollbackTransaction()
    {
        if (!IsTransaction) throw new Exception(ErrorNotOpenTransaction);

        IsTransaction = false;
    }
}
