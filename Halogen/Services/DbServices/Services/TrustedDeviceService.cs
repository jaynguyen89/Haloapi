using Halogen.DbContexts;
using Halogen.DbModels;
using Halogen.Services.DbServices.Interfaces;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Logger;
using Microsoft.EntityFrameworkCore;

namespace Halogen.Services.DbServices.Services; 

public sealed class TrustedDeviceService: DbServiceBase, ITrustedDeviceService {
    
    public TrustedDeviceService(
        ILoggerService logger,
        HalogenDbContext dbContext
    ): base(logger, dbContext) { }

    public async Task<TrustedDevice[]?> GetTrustedDevicesForAccount(string accountId) {
        _logger.Log(new LoggerBinding<TrustedDeviceService> { Location = nameof(GetTrustedDevicesForAccount) });
        try {
            return await _dbContext.TrustedDevices.Where(x => x.AccountId.Equals(accountId)).ToArrayAsync();
        }
        catch (ArgumentNullException e) {
            _logger.Log(new LoggerBinding<TrustedDeviceService> {
                Location = $"{nameof(GetTrustedDevicesForAccount)}.{nameof(ArgumentNullException)}",
                Severity = Enums.LogSeverity.Error, Data = e,
            });
            return default;
        }
    }
}