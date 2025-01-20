using Halogen.DbContexts;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared.Logger;

namespace Halogen.Services.DbServices.Services; 

public sealed class ContextService: DbServiceBase, IContextService {

    public ContextService(
        ILoggerService logger,
        HalogenDbContext dbContext
    ): base(logger, dbContext) { }

    public async Task StartTransaction() {
        _logger.Log(new LoggerBinding<ContextService> { Location = nameof(StartTransaction) });
        if (_dbContext.Database.CurrentTransaction is not null)
            await RevertTransaction();
        
        await _dbContext.Database.BeginTransactionAsync();
    }

    public async Task ConfirmTransaction() {
        _logger.Log(new LoggerBinding<ContextService> { Location = nameof(ConfirmTransaction) });
        await _dbContext.Database.CommitTransactionAsync();
    }

    public async Task RevertTransaction() {
        _logger.Log(new LoggerBinding<ContextService> { Location = nameof(RevertTransaction) });
        await _dbContext.Database.RollbackTransactionAsync();
    }
}