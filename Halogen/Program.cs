using System.Reflection;
using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using AmazonLibrary;
using AssistantLibrary;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AWS.Logger;
using Halogen.Parsers;
using HelperLibrary;
using HelperLibrary.Shared;
using MediaLibrary;
using MediaLibrary.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Halogen;

public static class Program {
    
    private const byte ConfigurationsRefreshMinutesInterval = 60;
    private static IConfiguration _configuration = null!;
    
    public static void Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration
               .AddJsonFile("appsettings.json", true, true)
               .AddEnvironmentVariables(nameof(Halogen));

        builder.Host
               .UseServiceProviderFactory(new AutofacServiceProviderFactory())
               .ConfigureAppConfiguration(configurations => {
                   configurations.AddSystemsManager(
                       $"{Constants.FSlash}{nameof(Halogen)}",
                       new AWSOptions { Region = RegionEndpoint.USEast1 },
                       false,
                       TimeSpan.FromMinutes(ConfigurationsRefreshMinutesInterval)
                   );
                    
                   configurations.AddSystemsManager(
                       $"{Constants.FSlash}{nameof(AssistantLibrary)}",
                       new AWSOptions { Region = RegionEndpoint.USEast1 },
                       false,
                       TimeSpan.FromMinutes(ConfigurationsRefreshMinutesInterval)
                   );
                    
                   configurations.AddSystemsManager(
                       $"{Constants.FSlash}{nameof(AmazonLibrary)}",
                       new AWSOptions { Region = RegionEndpoint.USEast1 },
                       false,
                       TimeSpan.FromMinutes(ConfigurationsRefreshMinutesInterval)
                   );
                    
                   configurations.AddSystemsManager(
                       $"{Constants.FSlash}{nameof(MediaLibrary)}",
                       new AWSOptions { Region = RegionEndpoint.USEast1 },
                       false,
                       TimeSpan.FromMinutes(ConfigurationsRefreshMinutesInterval)
                   );
               });
        
        _configuration = builder.Configuration;
        var environment = _configuration.GetValue<string>($"{nameof(Halogen)}Environment");

        var shouldCheckCookieConsent = environment switch {
            Constants.Development => bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.CookieShouldCheckConsent)}")),
            Constants.Staging => bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.CookieShouldCheckConsent)}")),
            _ => bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.CookieShouldCheckConsent)}"))
        };

        builder.Services.Configure<CookiePolicyOptions>(options => options.CheckConsentNeeded = _ => shouldCheckCookieConsent);
        builder.Services.AddCors();
        builder.Services.AddControllers().AddControllersAsServices();

        var (apiVersion, apiName, apiDescription) = environment switch {
            Constants.Development => (
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{nameof(HalogenOptions.Development.SwaggerInfo)}{Constants.Colon}{nameof(HalogenOptions.Development.SwaggerInfo.Version)}"),
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{nameof(HalogenOptions.Development.SwaggerInfo)}{Constants.Colon}{nameof(HalogenOptions.Development.SwaggerInfo.ApiName)}"),
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{nameof(HalogenOptions.Development.SwaggerInfo)}{Constants.Colon}{nameof(HalogenOptions.Development.SwaggerInfo.Description)}")
            ),
            Constants.Staging => (
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{nameof(HalogenOptions.Staging.SwaggerInfo)}{Constants.Colon}{nameof(HalogenOptions.Staging.SwaggerInfo.Version)}"),
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{nameof(HalogenOptions.Staging.SwaggerInfo)}{Constants.Colon}{nameof(HalogenOptions.Staging.SwaggerInfo.ApiName)}"),
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{nameof(HalogenOptions.Staging.SwaggerInfo)}{Constants.Colon}{nameof(HalogenOptions.Staging.SwaggerInfo.Description)}")
            ),
            _ => (
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{nameof(HalogenOptions.Production.SwaggerInfo)}{Constants.Colon}{nameof(HalogenOptions.Production.SwaggerInfo.Version)}"),
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{nameof(HalogenOptions.Production.SwaggerInfo)}{Constants.Colon}{nameof(HalogenOptions.Production.SwaggerInfo.ApiName)}"),
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{nameof(HalogenOptions.Production.SwaggerInfo)}{Constants.Colon}{nameof(HalogenOptions.Production.SwaggerInfo.Description)}")
            )
        };

        builder.Services.AddSwaggerGen(options => {
            options.SwaggerDoc($"Halo v.{apiVersion}", new OpenApiInfo {
                Title = apiName,
                Description = apiDescription,
                Version = apiVersion
            });
            
            var swaggerXmlFile = $"{ Assembly.GetExecutingAssembly().GetName().Name }.xml";
            var swaggerXmlFilePath = Path.Combine(AppContext.BaseDirectory, swaggerXmlFile);
            options.IncludeXmlComments(swaggerXmlFilePath);

            options.OperationFilter<SwaggerXmlFormatter>();
            options.CustomSchemaIds(type => type.ToString());
        });

        builder.Services.AddAuthentication(options => {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options => {
            var (requireHttpsMetadata, saveToken) = environment switch {
                Constants.Development => (
                    _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.JwtSettings.RequireHttpsMetadata)}"),
                    _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.JwtSettings.SaveToken)}")
                ),
                Constants.Staging => (
                    _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.JwtSettings.RequireHttpsMetadata)}"),
                    _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.JwtSettings.SaveToken)}")
                ),
                _ => (
                    _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.JwtSettings.RequireHttpsMetadata)}"),
                    _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.JwtSettings.SaveToken)}")
                )
            };
            
            options.RequireHttpsMetadata = bool.Parse(requireHttpsMetadata ?? true.ToString());
            options.SaveToken = bool.Parse(saveToken ?? true.ToString());

            var (validateIssuer, validateIssuerSigningKey, validateAudience, validateLifetime, requireExpirationTime, ignoreTrailingSlash) = environment switch {
                Constants.Development => (
                    bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Development.JwtSettings.TokenValidationParameters.ValidateIssuers)}")),
                    bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Development.JwtSettings.TokenValidationParameters.ValidateIssuerSigningKey)}")),
                    bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Development.JwtSettings.TokenValidationParameters.ValidateAudience)}")),
                    bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Development.JwtSettings.TokenValidationParameters.ValidateLifetime)}")),
                    bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Development.JwtSettings.TokenValidationParameters.RequireExpirationTime)}")),
                    bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Development.JwtSettings.TokenValidationParameters.IgnoreTrailingSlashWhenValidatingAudience)}"))
                ),
                Constants.Staging => (
                    bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Staging.JwtSettings.TokenValidationParameters.ValidateIssuers)}")),
                    bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Staging.JwtSettings.TokenValidationParameters.ValidateIssuerSigningKey)}")),
                    bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Staging.JwtSettings.TokenValidationParameters.ValidateAudience)}")),
                    bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Staging.JwtSettings.TokenValidationParameters.ValidateLifetime)}")),
                    bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Staging.JwtSettings.TokenValidationParameters.RequireExpirationTime)}")),
                    bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Staging.JwtSettings.TokenValidationParameters.IgnoreTrailingSlashWhenValidatingAudience)}"))
                ),
                _ => (
                    bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Production.JwtSettings.TokenValidationParameters.ValidateIssuers)}")),
                    bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Production.JwtSettings.TokenValidationParameters.ValidateIssuerSigningKey)}")),
                    bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Production.JwtSettings.TokenValidationParameters.ValidateAudience)}")),
                    bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Production.JwtSettings.TokenValidationParameters.ValidateLifetime)}")),
                    bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Production.JwtSettings.TokenValidationParameters.RequireExpirationTime)}")),
                    bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Production.JwtSettings.TokenValidationParameters.IgnoreTrailingSlashWhenValidatingAudience)}"))
                )
            };
            
            var (clockSkew, validIssuers, validAudiences, issuerSigningKeys) = environment switch {
                Constants.Development => (
                    int.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Development.JwtSettings.TokenValidationParameters.ClockSkew)}")),
                    _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Development.JwtSettings.TokenValidationParameters.ValidIssuers)}"),
                    _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Development.JwtSettings.TokenValidationParameters.ValidAudiences)}"),
                    _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Development.JwtSettings.TokenValidationParameters.IssuerSigningKeys)}")
                ),
                Constants.Staging => (
                    int.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Staging.JwtSettings.TokenValidationParameters.ClockSkew)}")),
                    _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Staging.JwtSettings.TokenValidationParameters.ValidIssuers)}"),
                    _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Staging.JwtSettings.TokenValidationParameters.ValidAudiences)}"),
                    _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Staging.JwtSettings.TokenValidationParameters.IssuerSigningKeys)}")
                ),
                _ => (
                    int.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Production.JwtSettings.TokenValidationParameters.ClockSkew)}")),
                    _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Production.JwtSettings.TokenValidationParameters.ValidIssuers)}"),
                    _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Production.JwtSettings.TokenValidationParameters.ValidAudiences)}"),
                    _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Production.JwtSettings.TokenValidationParameters.IssuerSigningKeys)}")
                )
            };

            options.TokenValidationParameters = new TokenValidationParameters {
                ValidateIssuer = validateIssuer,
                ValidateIssuerSigningKey = validateIssuerSigningKey,
                ValidateAudience = validateAudience,
                ValidateLifetime = validateLifetime,
                RequireExpirationTime = requireExpirationTime,
                IgnoreTrailingSlashWhenValidatingAudience = ignoreTrailingSlash,
                ClockSkew = TimeSpan.FromMinutes(clockSkew),
                ValidIssuers = (validIssuers ?? string.Empty).Split(Constants.Semicolon),
                ValidAudiences = (validAudiences ?? string.Empty).Split(Constants.Semicolon),
                IssuerSigningKeys = (issuerSigningKeys ?? string.Empty).Split(Constants.Semicolon).Select(key => new SymmetricSecurityKey(key.EncodeDataAscii()))
            };
        });
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddAuthorization(options => {
            var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme);
            options.DefaultPolicy = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser().Build();
        });

        var (idleTimeout, isEssential, maxAge, expiration) = environment switch {
            Constants.Development => (
                int.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.SessionSettings)}{Constants.Colon}{nameof(HalogenOptions.Development.SessionSettings.IdleTimeout)}")),
                bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.SessionSettings)}{Constants.Colon}{nameof(HalogenOptions.Development.SessionSettings.IsEssential)}")),
                int.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.SessionSettings)}{Constants.Colon}{nameof(HalogenOptions.Development.SessionSettings.MaxAge)}")),
                int.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.SessionSettings)}{Constants.Colon}{nameof(HalogenOptions.Development.SessionSettings.Expiration)}"))
            ),
            Constants.Staging => (
                int.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.SessionSettings)}{Constants.Colon}{nameof(HalogenOptions.Staging.SessionSettings.IdleTimeout)}")),
                bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.SessionSettings)}{Constants.Colon}{nameof(HalogenOptions.Staging.SessionSettings.IsEssential)}")),
                int.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.SessionSettings)}{Constants.Colon}{nameof(HalogenOptions.Staging.SessionSettings.MaxAge)}")),
                int.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.SessionSettings)}{Constants.Colon}{nameof(HalogenOptions.Staging.SessionSettings.Expiration)}"))
            ),
            _ => (
                int.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.SessionSettings)}{Constants.Colon}{nameof(HalogenOptions.Production.SessionSettings.IdleTimeout)}")),
                bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.SessionSettings)}{Constants.Colon}{nameof(HalogenOptions.Production.SessionSettings.IsEssential)}")),
                int.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.SessionSettings)}{Constants.Colon}{nameof(HalogenOptions.Production.SessionSettings.MaxAge)}")),
                int.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.SessionSettings)}{Constants.Colon}{nameof(HalogenOptions.Production.SessionSettings.Expiration)}"))
            )
        };

        builder.Services.AddSession(options => {
            options.IdleTimeout = TimeSpan.FromMinutes(idleTimeout);
            options.Cookie.HttpOnly = !environment.Equals(Constants.Development);
            options.Cookie.IsEssential = isEssential;
            options.Cookie.MaxAge = TimeSpan.FromDays(maxAge);
            options.Cookie.Expiration = TimeSpan.FromDays(expiration);
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });

        builder.Services
               .AddMvc(options => options.EnableEndpointRouting = false)
               .AddSessionStateTempDataProvider();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddOptions();

        var (awsAccessKeyId, awsSecretAccessKey, awsRegion, awsLogGroupName) = environment switch {
            Constants.Development => (
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.ServerSettings)}{Constants.Colon}{nameof(HalogenOptions.Development.ServerSettings.AwsAccessKeyId)}"),
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.ServerSettings)}{Constants.Colon}{nameof(HalogenOptions.Development.ServerSettings.AwsSecretAccessKey)}"),
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.ServerSettings)}{Constants.Colon}{nameof(HalogenOptions.Development.ServerSettings.AwsRegion)}"),
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.ServerSettings)}{Constants.Colon}{nameof(HalogenOptions.Development.ServerSettings.AwsLogGroupName)}")
            ),
            Constants.Staging => (
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.ServerSettings)}{Constants.Colon}{nameof(HalogenOptions.Staging.ServerSettings.AwsAccessKeyId)}"),
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.ServerSettings)}{Constants.Colon}{nameof(HalogenOptions.Staging.ServerSettings.AwsSecretAccessKey)}"),
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.ServerSettings)}{Constants.Colon}{nameof(HalogenOptions.Staging.ServerSettings.AwsRegion)}"),
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.ServerSettings)}{Constants.Colon}{nameof(HalogenOptions.Staging.ServerSettings.AwsLogGroupName)}")
            ),
            _ => (
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.ServerSettings)}{Constants.Colon}{nameof(HalogenOptions.Production.ServerSettings.AwsAccessKeyId)}"),
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.ServerSettings)}{Constants.Colon}{nameof(HalogenOptions.Production.ServerSettings.AwsSecretAccessKey)}"),
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.ServerSettings)}{Constants.Colon}{nameof(HalogenOptions.Production.ServerSettings.AwsRegion)}"),
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.ServerSettings)}{Constants.Colon}{nameof(HalogenOptions.Production.ServerSettings.AwsLogGroupName)}")
            )
        };

        if (!environment.Equals(Constants.Development))
            builder.Services.AddLogging(options => options.AddAWSProvider(new AWSLoggerConfig {
                Region = awsRegion,
                LogGroup = awsLogGroupName,
                Credentials = new BasicAWSCredentials(awsAccessKeyId, awsSecretAccessKey)
            }));

        builder.Services.Configure<AmazonLibraryOptions>(_configuration.GetSection(nameof(AmazonLibraryOptions)));
        builder.Services.Configure<AmazonLibraryOptions>(_configuration.GetSection(nameof(AssistantLibraryOptions)));
        builder.Services.Configure<AmazonLibraryOptions>(_configuration.GetSection(nameof(MediaLibraryOptions)));
        builder.Services.Configure<AmazonLibraryOptions>(_configuration.GetSection(nameof(HalogenOptions)));

        var app = builder.Build();
        
        app.UseSwagger();
        app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", $"{apiName} {apiVersion}"));

        app.UseAuthentication();
        app.UseSession();

        var corsOrigins = environment switch {
            Constants.Development => _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.SessionSettings)}{Constants.Colon}{nameof(HalogenOptions.Development.SessionSettings.CorsOrigins)}"),
            Constants.Staging => _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.SessionSettings)}{Constants.Colon}{nameof(HalogenOptions.Staging.SessionSettings.CorsOrigins)}"),
            _ => _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.SessionSettings)}{Constants.Colon}{nameof(HalogenOptions.Production.SessionSettings.CorsOrigins)}")
        };
        
        app.UseCors(options => options.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins(corsOrigins));

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();
        
        app.MapControllers();
        app.MapControllerRoute("default", "{controller}/{action}/{id?}");

        app.Run();
    }

    public static void ConfigureContainer(ContainerBuilder builder) {
        builder.RegisterInstance(_configuration).As<IConfiguration>();
        builder.RegisterAssistantLibraryServices();
        builder.RegisterAmazonLibraryServices();
        builder.RegisterMediaLibraryServices();
        builder.RegisterHalogenServices();
    }
}