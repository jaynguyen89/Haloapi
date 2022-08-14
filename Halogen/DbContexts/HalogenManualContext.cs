using Halogen.Parsers;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using Microsoft.EntityFrameworkCore;

namespace Halogen.DbContexts;

public partial class HalogenDbContext {
    
    private readonly string _connectionString;

    public HalogenDbContext(IEcosystem ecosystem, IConfiguration configuration) {
        var environment = ecosystem.GetEnvironment();
        var (serverEndpoint, dbName, username, password) = environment switch {
            Constants.Local => (
                configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings.ServerEndpoint)}"),
                configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings.DbName)}"),
                string.Empty,
                string.Empty
            ),
            _ => (
                configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings.ServerEndpoint)}"),
                configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings.DbName)}"),
                configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings.Username)}"),
                configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings.Password)}")
            ),
        };

        _connectionString = environment.Equals(Constants.Local)
            ? $"Server={serverEndpoint};Database={dbName};Trusted_Connection=True;"
            : $"Server={serverEndpoint};Database={dbName};User={username};Password={password};Trusted_Connection=True;";
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        base.OnConfiguring(optionsBuilder);
        if (!optionsBuilder.IsConfigured) optionsBuilder.UseSqlServer(_connectionString);
    }
}

// Scaffold-DbContext "Server=(localdb)\MSSQLLocalDB;Database=HalogenDatabase;Trusted_Connection=True;" Microsoft.EntityFrameworkCore.SqlServer -ContextNamespace Halogen.DbContexts -ContextDir D:\Halogeno\Haloapi\Halogen\DbContexts -OutputDir D:\Halogeno\Haloapi\Halogen\DbModels -Force -Context HalogenDbContext
