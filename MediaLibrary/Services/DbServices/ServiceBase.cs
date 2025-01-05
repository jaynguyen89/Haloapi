using System.Net.Http.Headers;
using Halogen;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;
using HelperLibrary.Shared.Logger;
using MediaLibrary.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MediaLibrary.Services.DbServices;

public class ServiceBase {

    protected readonly ILoggerService _logger;
    protected readonly MediaLibraryDbContext _dbContext;
    protected readonly MediaRoutePath _routePath;
    protected readonly HttpClient _httpClient = new();

    internal ServiceBase(
        ILoggerService logger,
        IConfiguration configuration,
        MediaLibraryDbContext dbContext,
        string environment
    ) {
        _logger = logger;
        _dbContext = dbContext;
        
        ParseRoutePath(configuration, environment, out _routePath);

        var baseUri = configuration.AsEnumerable().Single(x => x.Key.Equals($"{nameof(MediaLibraryOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(MediaLibraryOptions.Local.HttpClientBaseUri)}")).Value;
        
        _httpClient.BaseAddress = new Uri(baseUri!);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentTypes["json"]));
    }

    protected async Task<string?> SetApiToken(string accountId, string target) {
        _logger.Log(new LoggerBinding<ServiceBase> { Location = nameof(SetApiToken) });

        var apiToken = StringHelpers.GenerateRandomString();
        var dbApiToken = new Apitoken {
            Id = StringHelpers.NewGuid(),
            AccountId = accountId,
            TokenString = apiToken,
            TargetEndpoint = target,
        };

        try {
            _dbContext.Apitokens.Add(dbApiToken);
            var result = await _dbContext.SaveChangesAsync();
            return result == 1 ? apiToken : default;
        }
        catch (DbUpdateException e) {
            _logger.Log(new LoggerBinding<ServiceBase> {
                Location = $"{nameof(SetApiToken)}.{nameof(DbUpdateException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }

    private static void ParseRoutePath(IConfiguration configuration, string environment, out MediaRoutePath routePath) {
        var (avatar, cover, attachment) = (
            configuration.AsEnumerable().Single(x => x.Key.Equals($"{nameof(MediaLibraryOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(MediaLibraryOptions.Local.MediaRoutePath)}{Constants.Colon}{nameof(MediaLibraryOptions.Local.MediaRoutePath.Avatar)}")).Value,
            configuration.AsEnumerable().Single(x => x.Key.Equals($"{nameof(MediaLibraryOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(MediaLibraryOptions.Local.MediaRoutePath)}{Constants.Colon}{nameof(MediaLibraryOptions.Local.MediaRoutePath.Cover)}")).Value,
            configuration.AsEnumerable().Single(x => x.Key.Equals($"{nameof(MediaLibraryOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(MediaLibraryOptions.Local.MediaRoutePath)}{Constants.Colon}{nameof(MediaLibraryOptions.Local.MediaRoutePath.Attachment)}")).Value
        );

        routePath = new MediaRoutePath {
            Avatar = avatar!,
            Cover = cover!,
            Attachment = attachment!,
        };
    }
}
