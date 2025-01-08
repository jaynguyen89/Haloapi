namespace Halogen.Services.AppServices.Interfaces;

public interface IJwtService {
    
    /// <summary>
    /// To generate the JWT token to be used as Bearer token for client requests.
    /// </summary>
    /// <param name="claims">Dictionary:string:string</param>
    /// <returns>string</returns>
    string GenerateRequestAuthenticationToken(Dictionary<string, string> claims);
}