using FeedNews.Domain.Entities;

namespace FeedNews.Application.Common.Repositories;

public interface IAccountRepository : IBaseRepository<Account>
{
    Account? GetCustomerAccount(string email, string password);
}