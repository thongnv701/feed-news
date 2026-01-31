using FeedNews.Application.Common.Repositories;
using FeedNews.Domain.Entities;

namespace FeedNews.Infrastructure.Persistence.Repositories;

public class RoleRepository : BaseRepository<Role>, IRoleRepository
{
    public RoleRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}