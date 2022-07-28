using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Halogen.Parsers;
using Halogen.Services.AppServices.Interfaces;
using HelperLibrary;
using HelperLibrary.Shared;
using Microsoft.IdentityModel.Tokens;

namespace Halogen.Services.AppServices.Services; 

internal sealed class JwtService: IJwtService {
    
    private readonly ILogger<JwtService> _logger;
    private readonly IConfiguration _configuration;

    public JwtService(
        ILogger<JwtService> logger,
        IConfiguration configuration
    ) {
        _logger = logger;
        _configuration = configuration;
    }

    public string GenerateRequestAuthenticationToken(Dictionary<string, string> claims) {
        _logger.LogInformation($"{ nameof(JwtService) }.{ nameof(GenerateRequestAuthenticationToken) }: Service started.");

        var jwtClaims = claims.Select(claim => new Claim(claim.Key, claim.Value));
        
        var environment = _configuration.GetValue<string>($"{nameof(Halogen)}Environment");
        var (issuers, audiences, signingKeys, expiration) = environment switch {
            Constants.Development => (
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Development.JwtSettings.TokenValidationParameters.ValidIssuers)}"),
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Development.JwtSettings.TokenValidationParameters.ValidAudiences)}"),
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Development.JwtSettings.TokenValidationParameters.IssuerSigningKeys)}"),
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Development.JwtSettings.TokenValidationParameters.Expiration)}")
            ),
            Constants.Staging => (
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Staging.JwtSettings.TokenValidationParameters.ValidIssuers)}"),
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Staging.JwtSettings.TokenValidationParameters.ValidAudiences)}"),
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Staging.JwtSettings.TokenValidationParameters.IssuerSigningKeys)}"),
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Staging.JwtSettings.TokenValidationParameters.Expiration)}")
            ),
            Constants.Production => (
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Production.JwtSettings.TokenValidationParameters.ValidIssuers)}"),
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Production.JwtSettings.TokenValidationParameters.ValidAudiences)}"),
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Production.JwtSettings.TokenValidationParameters.IssuerSigningKeys)}"),
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Production.JwtSettings.TokenValidationParameters.Expiration)}")
            ),
            _ => (
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters.ValidIssuers)}"),
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters.ValidAudiences)}"),
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters.IssuerSigningKeys)}"),
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters.Expiration)}")
            )
        };

        var jwtToken = new JwtSecurityToken(
            issuers.Split(Constants.Semicolon)[0],
            audiences.Split(Constants.Semicolon)[0],
            jwtClaims,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMinutes(int.Parse(expiration)),
            new SigningCredentials(
                new SymmetricSecurityKey(signingKeys.Split(Constants.Semicolon)[0].EncodeDataAscii()),
                SecurityAlgorithms.HmacSha512Signature
            )
        );
        
        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }
}