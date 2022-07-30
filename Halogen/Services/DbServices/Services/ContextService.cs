using Halogen.DbContexts;
using Halogen.Services.DbServices.Interfaces;

namespace Halogen.Services.DbServices.Services; 

public sealed class ContextService: IContextService {

    private readonly ILogger<ContextService> _logger;
    private readonly HalogenDbContext _dbContext;

    public ContextService(ILogger<ContextService> logger, HalogenDbContext dbContext) {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task StartTransaction() {
        _logger.LogInformation("DB Transaction begins");
        await _dbContext.Database.BeginTransactionAsync();
    }

    public async Task ConfirmTransaction() {
        _logger.LogInformation("DB Transaction commits");
        await _dbContext.Database.CommitTransactionAsync();
    }

    public async Task RevertTransaction() {
        _logger.LogInformation("DB Transaction reverts");
        await _dbContext.Database.RollbackTransactionAsync();
    }
}