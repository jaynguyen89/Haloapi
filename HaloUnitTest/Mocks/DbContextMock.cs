using System.Runtime.InteropServices;
using Halogen.DbContexts;
using Halogen.DbModels;
using Microsoft.EntityFrameworkCore;

namespace HaloUnitTest.Mocks;

public sealed class HalogenDbContextMock {
    private static readonly Lazy<HalogenDbContextMock> DbContextMock = new(() => new HalogenDbContextMock());
    private static HalogenDbContext? _dbContext;
    
    private HalogenDbContextMock() { }

    public static HalogenDbContextMock Instance() => DbContextMock.Value;

    /* Intentional non-static method */
    public async Task<HalogenDbContext> MemoryDbContext(bool withMockData = true) {
        if (_dbContext is not null) return _dbContext;
        
        var dbOptions = new DbContextOptionsBuilder<HalogenDbContext>()
            .UseInMemoryDatabase("HalogenDbMock")
            .Options;
        _dbContext = new HalogenDbContext(dbOptions);

        if (withMockData) await SetDbContextData();
        return _dbContext;
    }

    /* Intentional non-static method */
    public async Task<HalogenDbContext> SqlDbContext(bool withMockData = true) {
        if (_dbContext is not null) return _dbContext;
        
        var connectionString = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "Server=(localdb)\\MSSQLLocalDB;Database=HalogenDatabaseTest;Trusted_Connection=True;"
            : "Server=localhost,1433;Database=HalogenDatabaseTest;User=sa;Password=adm1nP@ssw0rd;Trusted_Connection=False;Persist Security Info=False;Encrypt=False;";
        
        var dbOptions = new DbContextOptionsBuilder<HalogenDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        _dbContext = new HalogenDbContext(dbOptions);

        if (withMockData) {
            await _dbContext.AccountRoles.ExecuteDeleteAsync();
            await _dbContext.Accounts.ExecuteDeleteAsync();
            await _dbContext.Roles.ExecuteDeleteAsync();
            
            await SetDbContextData();
        }
        return _dbContext;
    }

    private static async Task SetDbContextData() {
        if (_dbContext is null) throw new Exception("_dbContext is null while adding mock data.");
        
        var dataMocks = new DataMocks();
        await _dbContext.Accounts.AddRangeAsync(dataMocks.Accounts);
        await _dbContext.Roles.AddRangeAsync(dataMocks.Roles);
        await _dbContext.AccountRoles.AddRangeAsync(dataMocks.AccountRoles);
        // await _dbContext.Addresses.AddRangeAsync(dataMocks.Addresses);
        // await _dbContext.Challenges.AddRangeAsync(dataMocks.Challenges);
        // await _dbContext.ChallengeResponses.AddRangeAsync(dataMocks.ChallengeResponses);
        // await _dbContext.Currencies.AddRangeAsync(dataMocks.Currencies);
        // await _dbContext.Localities.AddRangeAsync(dataMocks.Localities);
        // await _dbContext.LocalityDivisions.AddRangeAsync(dataMocks.LocalityDivisions);
        // await _dbContext.Preferences.AddRangeAsync(dataMocks.Preferences);
        // await _dbContext.Profiles.AddRangeAsync(dataMocks.Profiles);
        // await _dbContext.ProfileAddresses.AddRangeAsync(dataMocks.ProfileAddresses);
        // await _dbContext.TrustedDevices.AddRangeAsync(dataMocks.TrustedDevices);
        
        await _dbContext.SaveChangesAsync();
    }

    /*
     * Intentional non-static method.
     * To set any additional data into the DbContext if needed.
     */
    public async Task WithData(Dictionary<string, List<object>> data) {
        if (_dbContext is null) throw new Exception("_dbContext is null, call DbContext() to initialize it first.");
        
        foreach (var (modelName, mockData) in data)
            switch (modelName) {
                case nameof(Account):
                    await _dbContext.Accounts.AddRangeAsync(mockData.OfType<Account>());
                    break;
                case nameof(AccountRole):
                    await _dbContext.AccountRoles.AddRangeAsync(mockData.OfType<AccountRole>());
                    break;
                case nameof(Address):
                    await _dbContext.Addresses.AddRangeAsync(mockData.OfType<Address>());
                    break;
                case nameof(Challenge):
                    await _dbContext.Challenges.AddRangeAsync(mockData.OfType<Challenge>());
                    break;
                case nameof(ChallengeResponse):
                    await _dbContext.ChallengeResponses.AddRangeAsync(mockData.OfType<ChallengeResponse>());
                    break;
                case nameof(Currency):
                    await _dbContext.Currencies.AddRangeAsync(mockData.OfType<Currency>());
                    break;
                case nameof(Locality):
                    await _dbContext.Localities.AddRangeAsync(mockData.OfType<Locality>());
                    break;
                case nameof(LocalityDivision):
                    await _dbContext.LocalityDivisions.AddRangeAsync(mockData.OfType<LocalityDivision>());
                    break;
                case nameof(Preference):
                    await _dbContext.Preferences.AddRangeAsync(mockData.OfType<Preference>());
                    break;
                case nameof(Profile):
                    await _dbContext.Profiles.AddRangeAsync(mockData.OfType<Profile>());
                    break;
                case nameof(ProfileAddress):
                    await _dbContext.ProfileAddresses.AddRangeAsync(mockData.OfType<ProfileAddress>());
                    break;
                case nameof(Role):
                    await _dbContext.Roles.AddRangeAsync(mockData.OfType<Role>());
                    break;
                case nameof(TrustedDevice):
                    await _dbContext.TrustedDevices.AddRangeAsync(mockData.OfType<TrustedDevice>());
                    break;
            }

        await _dbContext.SaveChangesAsync();
    }
}