namespace Halogen.Services.AppServices.Interfaces;

public interface ISessionService {

    /// <summary>
    /// To set data into HttpContext.Session
    /// </summary>
    /// <param name="data">object</param>
    /// <param name="key">string</param>
    void Set(object data, string key);
    
    /// <summary>
    /// To retrieve data from HttpContext.Session
    /// </summary>
    /// <param name="key">string</param>
    /// <typeparam name="T">type</typeparam>
    /// <returns>T?</returns>
    T? Get<T>(string key);

    /// <summary>
    /// To remove an entry from HttpContext.Session
    /// </summary>
    /// <param name="key">string</param>
    void Remove(string key);

    /// <summary>
    /// To clear everything in the HttpContext.Session
    /// </summary>
    void Clear();
}