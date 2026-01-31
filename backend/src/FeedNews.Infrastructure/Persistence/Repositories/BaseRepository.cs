using System.Linq.Expressions;
using FeedNews.Application.Common.Repositories;
using FeedNews.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FeedNews.Infrastructure.Persistence.Repositories;

public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : BaseEntity
{
    private const string ErrorMessage = "Haven't any transaction";
    private readonly UnitOfWork _unitOfWork;

    public BaseRepository(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork as UnitOfWork
                      ?? throw new ArgumentNullException(nameof(unitOfWork), "UnitOfWork is null");
    }

    public DbSet<TEntity> DbSet => _unitOfWork.Context.Set<TEntity>();

    public async Task AddAsync(TEntity entity)
    {
        if (!_unitOfWork.IsTransaction) throw new InvalidOperationException(ErrorMessage);

        await DbSet.AddAsync(entity).ConfigureAwait(false);
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities)
    {
        if (!_unitOfWork.IsTransaction) throw new InvalidOperationException(ErrorMessage);

        await DbSet.AddRangeAsync(entities).ConfigureAwait(false);
    }

    public bool Any()
    {
        return DbSet.Any();
    }

    public Task<IQueryable<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        List<Expression<Func<TEntity, object>>>? includes = null,
        bool disableTracking = false)
    {
        return Task.FromResult(Get(predicate, orderBy, includes, disableTracking));
    }

    public IQueryable<TEntity> Get(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        List<Expression<Func<TEntity, object>>>? includes = null,
        bool disableTracking = false)
    {
        IQueryable<TEntity> query = DbSet;

        if (disableTracking) query = query.AsNoTracking();

        if (includes != null) query = includes.Aggregate(query, (current, include) => current.Include(include));

        if (predicate != null) query = query.Where(predicate);

        return orderBy != null
            ? orderBy(query).AsQueryable()
            : query.AsQueryable();
    }

    public void Remove(TEntity entity)
    {
        if (!_unitOfWork.IsTransaction) throw new InvalidOperationException(ErrorMessage);

        DbSet.Remove(entity);
    }

    public void RemoveRange(IEnumerable<TEntity> entities)
    {
        if (!_unitOfWork.IsTransaction) throw new InvalidOperationException(ErrorMessage);

        DbSet.RemoveRange(entities);
    }

    public TEntity Update(TEntity entity)
    {
        if (!_unitOfWork.IsTransaction) throw new InvalidOperationException(ErrorMessage);

        DbSet.Attach(entity);
        return entity;
    }

    public IEnumerable<TEntity> UpdateRange(IEnumerable<TEntity> entities)
    {
        if (!_unitOfWork.IsTransaction) throw new InvalidOperationException(ErrorMessage);

        DbSet.AttachRange(entities);
        return entities;
    }

    public async Task<TEntity?> GetByIdAsync(object id)
    {
        return await DbSet.FindAsync(id);
    }

    public TEntity? GetById(object id)
    {
        return DbSet.Find(id);
    }
}