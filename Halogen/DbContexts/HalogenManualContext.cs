using Halogen.Parsers;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Halogen.DbContexts;

public partial class HalogenDbContext {
    
    private readonly string _connectionString;

    public HalogenDbContext(IEcosystem ecosystem, IOptions<HalogenOptions> options) {
        var environment = ecosystem.GetEnvironment();
        var (serverEndpoint, dbName, username, password) = environment switch {
            Constants.Development => (
                options.Value.Dev.DbSettings.ServerEndpoint,
                options.Value.Dev.DbSettings.DbName,
                string.Empty,
                string.Empty
            ),
            Constants.Staging => (
                options.Value.Stg.DbSettings.ServerEndpoint,
                options.Value.Stg.DbSettings.DbName,
                options.Value.Stg.DbSettings.Username,
                options.Value.Stg.DbSettings.Password
            ),
            _ => (
                options.Value.Prod.DbSettings.ServerEndpoint,
                options.Value.Prod.DbSettings.DbName,
                options.Value.Prod.DbSettings.Username,
                options.Value.Prod.DbSettings.Password
            )
        };

        _connectionString = environment.Equals(Constants.Development)
            ? $"Server={serverEndpoint};Database={dbName};Trusted_Connection=True;"
            : $"Server={serverEndpoint};Database={dbName};User={username};Password={password};Trusted_Connection=True;";
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        base.OnConfiguring(optionsBuilder);
        if (!optionsBuilder.IsConfigured) optionsBuilder.UseSqlServer(_connectionString);
    }
}

// Scaffold-DbContext "Server=(localdb)\MSSQLLocalDB;Database=HalogenDatabase;Trusted_Connection=True;" Microsoft.EntityFrameworkCore.SqlServer -ContextNamespace Halogen.DbContexts -ContextDir D:\Halogeno\Haloapi\Halogen\DbContexts -OutputDir D:\Halogeno\Haloapi\Halogen\DbModels -Force -Context HalogenDbContext
