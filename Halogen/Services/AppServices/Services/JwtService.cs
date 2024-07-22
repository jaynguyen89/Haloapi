using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Halogen.Bindings;
using Halogen.Services.AppServices.Interfaces;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;
using HelperLibrary.Shared.Logger;
using Microsoft.IdentityModel.Tokens;

namespace Halogen.Services.AppServices.Services;

public sealed class JwtService: AppServiceBase, IJwtService {
    
    private readonly IConfiguration _configuration;

    public JwtService(
        ILoggerService logger,
        IConfiguration configuration
    ): base(logger) {
        _configuration = configuration;
    }

    public string GenerateRequestAuthenticationToken(Dictionary<string, string> claims) {
        _logger.Log(new LoggerBinding<JwtService> { Location = nameof(GenerateRequestAuthenticationToken) });

        var jwtClaims = claims.Select(claim => new Claim(claim.Key, claim.Value));
        
        var environment = _configuration.GetValue<string>($"{nameof(Halogen)}Environment");
        var (issuers, audiences, signingKeys, expiration) = (
            _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters.ValidIssuers)}"),
            _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters.ValidAudiences)}"),
            _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters.IssuerSigningKeys)}"),
            _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters.Expiration)}")
        );

        var jwtToken = new JwtSecurityToken(
            issuers!.Split(Constants.Semicolon)[0],
            audiences!.Split(Constants.Semicolon)[0],
            jwtClaims,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMinutes(int.Parse(expiration!)),
            new SigningCredentials(
                new SymmetricSecurityKey(signingKeys!.Split(Constants.Semicolon)[0].EncodeDataAscii()),
                SecurityAlgorithms.HmacSha512Signature
            )
        );
        
        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }
}