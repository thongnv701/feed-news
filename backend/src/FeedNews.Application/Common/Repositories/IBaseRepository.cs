using System.Linq.Expressions;
using FeedNews.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FeedNews.Application.Common.Repositories;

public interface IBaseRepository<TEntity> where TEntity : BaseEntity
{
    public DbSet<TEntity> DbSet { get; }

    Task<IQueryable<TEntity>> GetAsync(Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        List<Expression<Func<TEntity, object>>>? includes = null,
        bool disableTracking = false);

    IQueryable<TEntity> Get(Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        List<Expression<Func<TEntity, object>>>? includes = null,
        bool disableTracking = false);

    Task<TEntity?> GetByIdAsync(object id);

    TEntity? GetById(object id);

    TEntity Update(TEntity entity);

    IEnumerable<TEntity> UpdateRange(IEnumerable<TEntity> entities);

    Task AddAsync(TEntity entity);

    Task AddRangeAsync(IEnumerable<TEntity> entities);

    void Remove(TEntity entity);

    void RemoveRange(IEnumerable<TEntity> entities);

    bool Any();
}