using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MediaLibrary.DbContexts;

public partial class MediaLibraryDbContext {
    
    private readonly string _connectionString = "";

    public MediaLibraryDbContext(IEcosystem ecosystem, IConfiguration configuration) {
        var environment = ecosystem.GetEnvironment();
        
        var (endpoint, _, dbName, username, password) = (
            configuration.AsEnumerable().Single(x => x.Key.Equals($"{nameof(MediaLibraryOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(MediaLibraryOptions.Local.DbSettings)}{Constants.Colon}{nameof(MediaLibraryOptions.Local.DbSettings.Endpoint)}")).Value,
            configuration.AsEnumerable().Single(x => x.Key.Equals($"{nameof(MediaLibraryOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(MediaLibraryOptions.Local.DbSettings)}{Constants.Colon}{nameof(MediaLibraryOptions.Local.DbSettings.Port)}")).Value,
            configuration.AsEnumerable().Single(x => x.Key.Equals($"{nameof(MediaLibraryOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(MediaLibraryOptions.Local.DbSettings)}{Constants.Colon}{nameof(MediaLibraryOptions.Local.DbSettings.DbName)}")).Value,
            configuration.AsEnumerable().Single(x => x.Key.Equals($"{nameof(MediaLibraryOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(MediaLibraryOptions.Local.DbSettings)}{Constants.Colon}{nameof(MediaLibraryOptions.Local.DbSettings.Username)}")).Value,
            configuration.AsEnumerable().Single(x => x.Key.Equals($"{nameof(MediaLibraryOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(MediaLibraryOptions.Local.DbSettings)}{Constants.Colon}{nameof(MediaLibraryOptions.Local.DbSettings.Password)}")).Value
        );

        _connectionString = $"server={endpoint};user={username};password={password};database={dbName}";
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseMySql(_connectionString, ServerVersion.Parse("10.4.32-mariadb"));
}

// dotnet ef dbcontext scaffold "Server=localhost;User=jayng;Password=Jay181989!;Database=halomedia" "Pomelo.EntityFrameworkCore.MySql" --context-namespace MediaLibrary.DbContexts --context-dir E:\Halogeno\Haloapi\MediaLibrary\DbContexts --output-dir E:\Halogeno\Haloapi\MediaLibrary\DbModels --context MediaLibraryDbContext
