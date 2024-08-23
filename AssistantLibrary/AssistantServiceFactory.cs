using System.Reflection;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Helpers;
using Microsoft.Extensions.Configuration;
using HelperLibrary.Shared.Logger;

namespace AssistantLibrary;

public sealed class AssistantServiceFactory: IAssistantServiceFactory {

    private readonly IEcosystem _ecosystem;
    private readonly ILoggerService _logger;
    private readonly IConfiguration _configuration;
    
    private readonly Lazy<Dictionary<string, object>> _services = new(() => new Dictionary<string, object>());

    public AssistantServiceFactory(
        IEcosystem ecosystem,
        ILoggerService logger,
        IConfiguration configuration
    ) {
        _ecosystem = ecosystem;
        _logger = logger;
        _configuration = configuration;
    }

    public T? GetService<T>() {
        _logger.Log(new LoggerBinding<AssistantServiceFactory> { Location = nameof(GetService) });
        try {
            var serviceKey = typeof(T).FullName!;
            if (_services.Value.ContainsKey(serviceKey)) return (T)_services.Value!.GetDictionaryValue(serviceKey)!;
            
            var service = (T)Activator.CreateInstance(typeof(T), _ecosystem, _logger, _configuration)!;
            
            _services.Value!.Add(serviceKey, service);
            return service;
        }
        catch (ArgumentException e) {
            _logger.Log(new LoggerBinding<AssistantServiceFactory> {
                Location = $"{nameof(GetService)}.{nameof(ArgumentException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
        catch (NotSupportedException e) {
            _logger.Log(new LoggerBinding<AssistantServiceFactory> {
                Location = $"{nameof(GetService)}.{nameof(NotSupportedException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
        catch (TargetInvocationException e) {
            _logger.Log(new LoggerBinding<AssistantServiceFactory> {
                Location = $"{nameof(GetService)}.{nameof(TargetInvocationException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
        catch (MethodAccessException e) {
            _logger.Log(new LoggerBinding<AssistantServiceFactory> {
                Location = $"{nameof(GetService)}.{nameof(MethodAccessException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
        catch (MemberAccessException e) {
            _logger.Log(new LoggerBinding<AssistantServiceFactory> {
                Location = $"{nameof(GetService)}.{nameof(MemberAccessException)}",
                Severity = Enums.LogSeverity.Error, E = e,
            });
            return default;
        }
    }
}