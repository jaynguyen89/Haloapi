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
    private readonly IConfiguration _configuration;
    private readonly MediaLibraryDbContext _dbContext;
    private readonly string _environment;
    
    private readonly Lazy<Dictionary<string, object>> _services = new(() => new Dictionary<string, object>());

    public MediaServiceFactory(
        ILoggerService logger,
        IConfiguration configuration,
        MediaLibraryDbContext dbContext,
        IEcosystem ecosystem
    ) {
        _logger = logger;
        _configuration = configuration;
        _dbContext = dbContext;
        _environment = ecosystem.GetEnvironment();
    }

    public T? GetService<T>() {
        _logger.Log(new LoggerBinding<MediaServiceFactory> { Location = nameof(GetService) });

        try {
            var serviceKey = typeof(T).FullName!;
            if (_services.Value.ContainsKey(serviceKey)) return (T)_services.Value!.GetDictionaryValue(serviceKey)!;
            
            var service = (T)Activator.CreateInstance(typeof(T), _logger, _configuration, _dbContext, _environment)!;
            
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
}
