namespace Halogen.Services.AppServices.Interfaces; 

internal interface IJwtService {
    
    string GenerateRequestAuthenticationToken(Dictionary<string, string> claims);
}