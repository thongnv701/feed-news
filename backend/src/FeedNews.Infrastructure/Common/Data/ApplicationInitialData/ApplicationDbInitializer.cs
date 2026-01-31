namespace FeedNews.Infrastructure.Common.Data.ApplicationInitialData;

public class ApplicationDbInitializer
{
    /*private readonly ILogger<ApplicationDbInitializer> _logger;
    private readonly IRoleRepository _roleRepository;
    private readonly IAccountRepository _accountRepository;
    private IUnitOfWork _unitOfWork;

    public ApplicationDbInitializer(ILogger<ApplicationDbInitializer> logger, IRoleRepository roleRepository, IAccountRepository accountRepository, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _roleRepository = roleRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task SeedAsync()
    {
        await this._unitOfWork.BeginTransactionAsync();
        try
        {
            //if (!_buildingRepository.Any())
            //{
            //    await this._roleRepository.AddRangeAsync(Seed.DefaultRoles);
            //}

            //if (!_buildingRepository.Any())
            //{
            //    await this._buildingRepository.AddRangeAsync(Seed.DefaultBuildings);
            //}

            //if (!_accountRepository.Any())
            //{
            //    await this._accountRepository.AddRangeAsync(Seed.DefaultAccounts);
            //}

            await this._unitOfWork.CommitTransactionAsync();
            this._logger.LogInformation("Seed Data Success");
        }
        catch
        {
            this._unitOfWork.RollbackTransaction();
            this._logger.LogInformation("Seed Data Fail");
            throw;
        }
    }*/
}