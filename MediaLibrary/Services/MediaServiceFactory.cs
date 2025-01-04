using System.Reflection;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Helpers;
using HelperLibrary.Shared.Logger;
using MediaLibrary.DbContexts;
using Microsoft.Extensions.Configuration;

namespace MediaLibrary.Services;

public class MediaServiceFactory: IMediaServiceFactory {
    
    private readonly ILoggerService _logger;
    private readonly MediaLibraryDbContext _dbContext;
    private readonly MediaRoutePath _routePath;
    
    private readonly Lazy<Dictionary<string, object>> _services = new(() => new Dictionary<string, object>());

    public MediaServiceFactory(
        ILoggerService logger,
        IConfiguration configuration,
        MediaLibraryDbContext dbContext,
        IEcosystem ecosystem
    ) {
        _logger = logger;
        _dbContext = dbContext;
        ParseRoutePath(configuration, ecosystem.GetEnvironment(), out _routePath);
    }

    public T? GetService<T>() {
        _logger.Log(new LoggerBinding<MediaServiceFactory> { Location = nameof(GetService) });

        try {
            var serviceKey = typeof(T).FullName!;
            if (_services.Value.ContainsKey(serviceKey)) return (T)_services.Value!.GetDictionaryValue(serviceKey)!;
            
            var service = (T)Activator.CreateInstance(typeof(T), _logger, _dbContext, _routePath)!;
            
            _services.Value.Add(serviceKey, service);
            return service;
        }
        catch (ArgumentException e) {
            _logger.Log(new LoggerBinding<MediaServiceFactory> {
                Location = $"{nameof(GetService)}.{nameof(ArgumentException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
        catch (NotSupportedException e) {
            _logger.Log(new LoggerBinding<MediaServiceFactory> {
                Location = $"{nameof(GetService)}.{nameof(NotSupportedException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
        catch (TargetInvocationException e) {
            _logger.Log(new LoggerBinding<MediaServiceFactory> {
                Location = $"{nameof(GetService)}.{nameof(TargetInvocationException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
        catch (MethodAccessException e) {
            _logger.Log(new LoggerBinding<MediaServiceFactory> {
                Location = $"{nameof(GetService)}.{nameof(MethodAccessException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
        catch (MemberAccessException e) {
            _logger.Log(new LoggerBinding<MediaServiceFactory> {
                Location = $"{nameof(GetService)}.{nameof(MemberAccessException)}",
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
