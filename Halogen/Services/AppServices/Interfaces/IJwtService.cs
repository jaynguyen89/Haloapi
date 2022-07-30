namespace Halogen.Services.AppServices.Interfaces;

public interface IJwtService {
    
    string GenerateRequestAuthenticationToken(Dictionary<string, string> claims);
}