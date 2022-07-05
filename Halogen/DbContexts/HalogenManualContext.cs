using Halogen.Parsers;
using HelperLibrary.Shared;
using Microsoft.EntityFrameworkCore;

namespace Halogen.DbContexts;

public partial class HalogenDbContext {
    
    private readonly string _connectionString;

    public HalogenDbContext(IConfiguration configuration) {
        var environment = configuration.GetValue<string>($"{nameof(Halogen)}Environment");
        var (serverEndpoint, dbName, username, password) = environment switch {
            Constants.Development => (
                configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.DbSettings)}{Constants.Colon}{nameof(HalogenOptions.Development.DbSettings.ServerEndpoint)}"),
                configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.DbSettings)}{Constants.Colon}{nameof(HalogenOptions.Development.DbSettings.DbName)}"),
                string.Empty,
                string.Empty
            ),
            Constants.Staging => (
                configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.DbSettings)}{Constants.Colon}{nameof(HalogenOptions.Staging.DbSettings.ServerEndpoint)}"),
                configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.DbSettings)}{Constants.Colon}{nameof(HalogenOptions.Staging.DbSettings.DbName)}"),
                configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.DbSettings)}{Constants.Colon}{nameof(HalogenOptions.Staging.DbSettings.Username)}"),
                configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.DbSettings)}{Constants.Colon}{nameof(HalogenOptions.Staging.DbSettings.Password)}")
            ),
            _ => (
                configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.DbSettings)}{Constants.Colon}{nameof(HalogenOptions.Production.DbSettings.ServerEndpoint)}"),
                configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.DbSettings)}{Constants.Colon}{nameof(HalogenOptions.Production.DbSettings.DbName)}"),
                configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.DbSettings)}{Constants.Colon}{nameof(HalogenOptions.Production.DbSettings.Username)}"),
                configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.DbSettings)}{Constants.Colon}{nameof(HalogenOptions.Production.DbSettings.Password)}")
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
