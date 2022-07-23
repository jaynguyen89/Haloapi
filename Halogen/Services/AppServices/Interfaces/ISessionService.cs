namespace Halogen.Services.AppServices.Interfaces; 

internal interface ISessionService {

    void Set(object data, string key);
    
    T? Get<T>(string key);

    void Remove(string key);

    void Clear();
}