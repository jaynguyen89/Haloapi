using System.Runtime.InteropServices;
using Halogen.Bindings;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using Microsoft.EntityFrameworkCore;

namespace Halogen.DbContexts;

public partial class HalogenDbContext {
    
    private readonly string _connectionString = "";

    public HalogenDbContext(IEcosystem ecosystem, IConfiguration configuration) {
        var environment = ecosystem.GetEnvironment();
        var (winEndpoint, linEndpoint, port, dbName, username, password) = environment switch {
            Constants.Local => (
                configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings.WinEndpoint)}"),
                configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings.LinEndpoint)}"),
                configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings.Port)}"),
                configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings.DbName)}"),
                configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings.Username)}"),
                configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings.Password)}")
            ),
            _ => (
                configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings.WinEndpoint)}"),
                string.Empty,
                configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings.Port)}"),
                configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings.DbName)}"),
                configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings.Username)}"),
                configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.DbSettings.Password)}")
            ),
        };

        // ReSharper disable once InconsistentNaming
        var isWindowsOS = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        var isLocal = environment.Equals(Constants.Local);
        _connectionString = isWindowsOS
            ? $"Server={winEndpoint};Database={dbName};Trusted_Connection=True;"
            : $"Server={(isLocal ? $"{linEndpoint},{port}" : winEndpoint)};Database={dbName};User={username};Password={password};Trusted_Connection={(isLocal ? "False" : "True")};";
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        base.OnConfiguring(optionsBuilder);
        if (!optionsBuilder.IsConfigured)
            optionsBuilder
                .UseLazyLoadingProxies()
                .UseSqlServer(_connectionString);
    }
}

//Win: Scaffold-DbContext "Server=(localdb)\MSSQLLocalDB;Database=HalogenDatabase;Trusted_Connection=True;" Microsoft.EntityFrameworkCore.SqlServer -ContextNamespace Halogen.DbContexts -ContextDir DbContexts -OutputDir DbModels -Force -Context HalogenDbContext
//Win: dotnet ef dbcontext scaffold "Server=(localdb)\MSSQLLocalDB;Database=HalogenDatabase;Trusted_Connection=True;" Microsoft.EntityFrameworkCore.SqlServer --context-namespace Halogen.DbContexts --context-dir DbContexts --output-dir DbModels --force --context HalogenDbContext

//Lin: Scaffold-DbContext "Server=localhost;Database=HalogenDatabase;Trusted_Connection=True;User=sa;Password=adm1nP@ssword" Microsoft.EntityFrameworkCore.SqlServer -ContextNamespace Halogen.DbContexts -ContextDir DbContexts -OutputDir DbModels -Force -Context HalogenDbContext
//Lin: dotnet ef dbcontext scaffold "Server=localhost;Database=HalogenDatabase;Trusted_Connection=True;User=sa;Password=adm1nP@ssword" Microsoft.EntityFrameworkCore.SqlServer --context-namespace Halogen.DbContexts --context-dir DbContexts --output-dir DbModels --force --context HalogenDbContext

// docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=adm1nP@ssword" -e "MSSQL_PID=Express" -p 1433:1433 --name mssql_server -d mcr.microsoft.com/mssql/server:2022-latest
// docker run --name mongo_server -d mongodb/mongodb-community-server:latest
// docker run --name redis_server -d redis
