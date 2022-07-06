using System.Security.Claims;

namespace Halogen.Services.DbServices.Interfaces; 

internal interface IJwtService {
    
    string GenerateRequestAuthenticationToken(Dictionary<string, string> claims);
}