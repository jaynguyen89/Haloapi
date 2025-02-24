using System.Reflection;
using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using AmazonLibrary;
using AssistantLibrary;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AWS.Logger;
using Halogen.Attributes;
using Halogen.Auxiliaries;
using Halogen.Bindings;
using Halogen.Services;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Ecosystem;
using HelperLibrary.Shared.Helpers;
using MediaLibrary.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace Halogen;

public static class Program {
    
    private const byte ConfigurationsRefreshMinutesInterval = 60;
    private static IConfiguration _configuration = null!;

    private static string Environment { get; set; } = null!;
    private static string AwsAccessKeyId { get; set; } = null!;
    private static string AwsSecretAccessKey { get; set; } = null!;
    private static string AwsRegion { get; set; } = null!;

    public static void Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Configuration.AddEnvironmentVariables(nameof(Halogen));
        var (environment, awsAccessKeyId, awsSecretAccessKey, awsRegion, awsLogGroup) = (
            builder.Configuration.GetValue<string>($"{nameof(Halogen)}{Constants.Underscore}Environment"),
            builder.Configuration.GetValue<string>($"{nameof(Halogen)}{Constants.Underscore}{nameof(HalogenOptions.ServerSettings.AwsAccessKeyId)}"),
            builder.Configuration.GetValue<string>($"{nameof(Halogen)}{Constants.Underscore}{nameof(HalogenOptions.ServerSettings.AwsSecretAccessKey)}"),
            builder.Configuration.GetValue<string>($"{nameof(Halogen)}{Constants.Underscore}{nameof(HalogenOptions.ServerSettings.AwsRegion)}"),
            builder.Configuration.GetValue<string>($"{nameof(Halogen)}{Constants.Underscore}{nameof(HalogenOptions.ServerSettings.AwsLogGroupName)}")
        );

        Environment = environment!;
        AwsAccessKeyId = awsAccessKeyId!;
        AwsSecretAccessKey = awsSecretAccessKey!;
        AwsRegion = awsRegion!;

        builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        if (environment!.Equals(Constants.Local)) builder.Configuration.AddJsonFile("appsettings.json", true, true);
        else builder.Configuration.AddSystemsManager(
                            $"{Constants.FSlash}{nameof(Halogen)}",
                            new AWSOptions { Region = RegionEndpoint.USEast1 },
                            false,
                            TimeSpan.FromMinutes(ConfigurationsRefreshMinutesInterval)
                        ).AddSystemsManager(
                            $"{Constants.FSlash}{nameof(AssistantLibrary)}",
                            new AWSOptions { Region = RegionEndpoint.USEast1 },
                            false,
                            TimeSpan.FromMinutes(ConfigurationsRefreshMinutesInterval)
                        ).AddSystemsManager(
                            $"{Constants.FSlash}{nameof(AmazonLibrary)}",
                            new AWSOptions { Region = RegionEndpoint.USEast1 },
                            false,
                            TimeSpan.FromMinutes(ConfigurationsRefreshMinutesInterval)
                        ).AddSystemsManager(
                            $"{Constants.FSlash}{nameof(MediaLibrary)}",
                            new AWSOptions { Region = RegionEndpoint.USEast1 },
                            false,
                            TimeSpan.FromMinutes(ConfigurationsRefreshMinutesInterval)
                        );

        _configuration = builder.Configuration;
        builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory(ConfigureContainer));
        
        var shouldCheckCookieConsent = environment switch {
            Constants.Development => bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Development.CookieShouldCheckConsent)}")!),
            Constants.Staging => bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Staging.CookieShouldCheckConsent)}")!),
            Constants.Production => bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Production.CookieShouldCheckConsent)}")!),
            _ => bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.CookieShouldCheckConsent)}")!)
        };

        builder.Services.Configure<CookiePolicyOptions>(options => {
            options.CheckConsentNeeded = _ => shouldCheckCookieConsent;
            // options.MinimumSameSitePolicy = SameSiteMode.Strict;
        });
        builder.Services.AddCors();
        builder.Services.AddControllers().AddControllersAsServices();

        var (apiVersion, apiName, apiDescription) = (
            _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.SwaggerInfo)}{Constants.Colon}{nameof(HalogenOptions.Local.SwaggerInfo.Version)}"),
            _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.SwaggerInfo)}{Constants.Colon}{nameof(HalogenOptions.Local.SwaggerInfo.ApiName)}"),
            _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.SwaggerInfo)}{Constants.Colon}{nameof(HalogenOptions.Local.SwaggerInfo.Description)}")
        );
        
        builder.Services.AddSwaggerGen(options => {
            options.SwaggerDoc($"{apiVersion}", new OpenApiInfo {
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
        })
        .AddCookie(options => options.ExpireTimeSpan = TimeSpan.FromMinutes(600))
        .AddJwtBearer(options => {
            var (requireHttpsMetadata, saveToken) = (
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.RequireHttpsMetadata)}"),
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.SaveToken)}")
            );
            
            options.RequireHttpsMetadata = bool.Parse(requireHttpsMetadata ?? true.ToString());
            options.SaveToken = bool.Parse(saveToken ?? true.ToString());

            var (validateIssuer, validateIssuerSigningKey, validateAudience, validateLifetime, requireExpirationTime, ignoreTrailingSlash) = (
                bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters.ValidateIssuers)}")!),
                bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters.ValidateIssuerSigningKey)}")!),
                bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters.ValidateAudience)}")!),
                bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters.ValidateLifetime)}")!),
                bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters.RequireExpirationTime)}")!),
                bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters.IgnoreTrailingSlashWhenValidatingAudience)}")!)
            );
            
            var (clockSkew, validIssuers, validAudiences, issuerSigningKeys) = (
                int.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters.ClockSkew)}")!),
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters.ValidIssuers)}"),
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters.ValidAudiences)}"),
                _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters)}{Constants.Colon}{nameof(HalogenOptions.Local.JwtSettings.TokenValidationParameters.IssuerSigningKey)}")
            );

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
        
        //builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddAuthorization(options => {
            var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme);
            options.DefaultPolicy = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser().Build();
        });

        var (name, idleTimeout, isEssential, maxAge) = (
            _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.SessionSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.SessionSettings.Name)}")!,
            int.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.SessionSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.SessionSettings.IdleTimeout)}")!),
            bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.SessionSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.SessionSettings.IsEssential)}")!),
            int.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.SessionSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.SessionSettings.MaxAge)}")!)
        );
        
        builder.Services.AddSession(options => {
            options.Cookie.Name = name;
            options.IdleTimeout = TimeSpan.FromMinutes(idleTimeout);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = isEssential;
            options.Cookie.MaxAge = TimeSpan.FromDays(maxAge);
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });

        //builder.Services.AddScoped<AutoValidateAntiforgeryTokenAttribute>();
        builder.Services
            .AddMvc(options => {
                options.EnableEndpointRouting = false;
                //options.Filters.Add(typeof(AutoValidateAntiforgeryTokenAttribute));
            });

        if (!environment.Equals(Constants.Local))
            builder.Services.AddLogging(options => options.AddAWSProvider(new AWSLoggerConfig {
                Region = awsRegion,
                LogGroup = awsLogGroup,
                Credentials = new BasicAWSCredentials(awsAccessKeyId, awsSecretAccessKey)
            }));

        var isCacheEnabled = bool.Parse(_configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings.IsEnabled)}")!);
        if (isCacheEnabled)
            builder.Services.AddStackExchangeRedisCache(options => {
                var (endpoint, port, password, ssl, defaultDb, abortConnect, allowAdmin, instanceName) = (
                    _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings.Connection)}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings.Connection.Endpoint)}"),
                    _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings.Connection)}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings.Connection.Port)}"),
                    _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings.Connection)}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings.Connection.Password)}"),
                    _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings.Connection)}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings.Connection.Ssl)}"),
                    _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings.Connection)}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings.Connection.DefaultDb)}"),
                    _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings.Connection)}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings.Connection.AbortConnect)}"),
                    _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings.Connection)}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings.Connection.AllowAdmin)}"),
                    _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings.Connection)}{Constants.Colon}{nameof(HalogenOptions.Local.RedisCacheSettings.Connection.InstanceName)}")
                );

                var connectionString = $"{endpoint}{Constants.Colon}{port}";
                connectionString += password.IsString() ? $"{Constants.Comma}password={password}" : string.Empty;
                connectionString += ssl.IsString() ? $"{Constants.Comma}ssl={ssl}" : string.Empty;
                connectionString += defaultDb.IsString() ? $"{Constants.Comma}defaultDatabase={defaultDb}" : string.Empty;
                connectionString += abortConnect.IsString() ? $"{Constants.Comma}abortConnect={abortConnect}" : string.Empty;
                connectionString += allowAdmin.IsString() ? $"{Constants.Comma}allowAdmin={allowAdmin}" : string.Empty;

                options.Configuration = connectionString;
                options.InstanceName = instanceName;
            });

        builder.Services.AddOptions();
        
        builder.Services.AddScoped<RecaptchaAuthorize>();
        builder.Services.AddScoped<TwoFactorAuthorize>();
        builder.Services.AddScoped<RoleAuthorize>();
        builder.Services.AddScoped<AuthenticatedAuthorize>();
        builder.Services.AddScoped<AccountAndProfileAssociatedAuthorize>();

        var app = builder.Build();

        app.UseAuthentication();
        app.UseSession();
        
        app.UseSwagger();
        app.UseSwaggerUI(options => options.SwaggerEndpoint($"/swagger/{apiVersion}/swagger.json", $"{apiName} {apiVersion}"));
        
        app.UseMiddleware<GlobalErrorHandler>();

        app.UseCookiePolicy();
        
        var corsOrigins = _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.SessionSettings)}{Constants.Colon}{nameof(HalogenOptions.Local.SessionSettings.CorsOrigins)}");
        app.UseCors(options =>
            options.AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials()
                   .WithOrigins(corsOrigins!.Split(Constants.Comma))
        );
        
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();
        
        if (!environment.Equals(Constants.Local)) {
            var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
            _ = loggerFactory.AddAWSProvider(new AWSLoggerConfig {
                Region = awsRegion,
                LogGroup = awsLogGroup,
                Credentials = new BasicAWSCredentials(awsAccessKeyId, awsSecretAccessKey)
            });
        }
        else app.UseDeveloperExceptionPage();

        // if (!environment.Equals(Constants.Local)) {
        //     var antiForgeryHeaderName = _configuration.GetValue<string>($"{nameof(HalogenOptions)}{Constants.Colon}{environment}{Constants.Colon}{nameof(HalogenOptions.Local.CsrfHeaderName)}");
        //     var antiForgery = app.Services.GetRequiredService<IAntiforgery>();
        //     app.Use((context, next) => {
        //         var tokenSet = antiForgery.GetAndStoreTokens(context);
        //         context.Response.Cookies.Append(
        //             antiForgeryHeaderName,
        //             tokenSet.RequestToken!,
        //             new CookieOptions { HttpOnly = false }
        //         );
        //
        //         return next(context);
        //     });
        // }

        app.MapControllers();
        app.MapControllerRoute("default", "{controller}/{action}/{id?}");

        app.Run();
    }

    private static void ConfigureContainer(ContainerBuilder builder) {
        builder.RegisterInstance(builder).As<ContainerBuilder>();
        builder.RegisterInstance(_configuration).As<IConfiguration>();
        
        var useLongerId = bool.Parse(_configuration.GetValue<string>($"{nameof(Halogen)}{Constants.Underscore}{nameof(Ecosystem.UseLongerId)}")!);
        builder.RegisterInstance(new Ecosystem {
            Environment = Environment,
            UseLongerId = useLongerId,
            ServerSetting = new Ecosystem.ServerSettings {
                AwsRegion = AwsRegion,
                AwsAccessKeyId = AwsAccessKeyId,
                AwsSecretAccessKey = AwsSecretAccessKey
            }
        }).As<IEcosystem>().SingleInstance();
        
        builder.RegisterAssistantLibraryServices();
        builder.RegisterAmazonLibraryServices();
        builder.RegisterMediaLibraryServices();
        builder.RegisterHalogenServices();
    }
}