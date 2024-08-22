using System.Reflection;
using HelperLibrary.Attributes;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using Microsoft.Extensions.Configuration;
using HelperLibrary.Shared.Logger;

namespace AssistantLibrary;

[Todo("Refactor this class as described in comment below")]
/*
/// This class is registered to Autofac Builder as a Singleton dependency.
/// But each time we call GetService<T>(), a new instance of a service will be created, and it is bad.
/// So refactor this class to make method GetService<T>() return an existing instance of a service if any, otherwise, return a new instance of the service.
*/
public sealed class AssistantServiceFactory: IAssistantServiceFactory {

    private readonly IEcosystem _ecosystem;
    private readonly ILoggerService _logger;
    private readonly IConfiguration _configuration;

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
            return (T)Activator.CreateInstance(typeof(T), _ecosystem, _logger, _configuration)!;
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