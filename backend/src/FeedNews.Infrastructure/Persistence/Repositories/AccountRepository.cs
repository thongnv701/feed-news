using FeedNews.Application.Common.Repositories;
using FeedNews.Domain.Entities;

namespace FeedNews.Infrastructure.Persistence.Repositories;

public class AccountRepository : BaseRepository<Account>, IAccountRepository
{
    public AccountRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public Account? GetCustomerAccount(string email, string password)
    {
        return DbSet.SingleOrDefault(a => a.Email == email && a.Password == password);
    }
}