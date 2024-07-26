using Halogen.Services.AppServices.Interfaces;
using HelperLibrary.Shared.Logger;
using Newtonsoft.Json;

namespace Halogen.Services.AppServices.Services;

public class SessionService: AppServiceBase, ISessionService {

    private readonly ISession _session;

    public SessionService() { }

    public SessionService(
        IHttpContextAccessor httpContextAccessor,
        ILoggerService logger
    ) : base(logger) {
        if (httpContextAccessor.HttpContext is null) {
            logger.Log(new LoggerBinding<SessionService> { Location = nameof(SessionService) });
            throw new NullReferenceException();
        }
        
        _session = httpContextAccessor.HttpContext.Session;
    }

    public virtual void Set(object data, string key) {
        _logger.Log(new LoggerBinding<SessionService> { Location = nameof(Set) });

        var cache = JsonConvert.SerializeObject(data);
        _session.SetString(key, cache);
    }

    public virtual T? Get<T>(string key) {
        _logger.Log(new LoggerBinding<SessionService> { Location = nameof(Get) });
        
        var data = _session.GetString(key);
        return data is null ? default : JsonConvert.DeserializeObject<T>(data);
    }

    public virtual void Remove(string key) {
        _logger.Log(new LoggerBinding<SessionService> { Location = nameof(Remove) });
        _session.Remove(key);
    }

    public virtual void Clear() {
        _logger.Log(new LoggerBinding<SessionService> { Location = nameof(Clear) });
        _session.Clear();
    }
}