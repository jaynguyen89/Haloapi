#pragma warning disable 8618
namespace Halogen.Parsers;
    
public sealed class HalogenOptions {

    public class Development {
        
        public string CookieShouldCheckConsent { get; set; }
        
        public sealed class DbSettings {
            
            public string ServerEndpoint { get; set; }
            
            public string DbName { get; set; }
            
            public string Username { get; set; }
            
            public string Password { get; set; }
        }

        public sealed class ServerSettings {
            
            public string AwsAccessKeyId { get; set; }
            
            public string AwsSecretAccessKey { get; set; }
            
            public string AwsRegion { get; set; }
        
            public string AwsLogGroupName { get; set; }
        }

        public sealed class SwaggerInfo {
            
            public string Version { get; set; }
            
            public string ApiName { get; set; }

            public string Description { get; set; }
        }

        public sealed class JwtSettings {
        
            public string RequireHttpsMetadata { get; set; }
        
            public string SaveToken { get; set; }
        
            public sealed class TokenValidationParameters {
            
                public string ValidateIssuers { get; set; }
            
                public string ValidateIssuerSigningKey { get; set; }
            
                public string ValidateAudience { get; set; }
            
                public string ValidateLifetime { get; set; }
            
                public string RequireExpirationTime { get; set; }
            
                public string IgnoreTrailingSlashWhenValidatingAudience { get; set; }
            
                public string ClockSkew { get; set; }
            
                public string ValidIssuers { get; set; }
            
                public string ValidAudiences { get; set; }
            
                public string IssuerSigningKeys { get; set; }
                
                public string Expiration { get; set; }
            }
        }
        
        public sealed class SessionSettings {
            
            public string IdleTimeout { get; set; }
            
            public string IsEssential { get; set; }
            
            public string MaxAge { get; set; }
            
            public string Expiration { get; set; }
            
            public string CorsOrigins { get; set; }
        }
        
        public sealed class CacheSettings {

            public string IsEnabled { get; set; }
            
            public string SlidingExpiration { get; set; }
            
            public string AbsoluteExpiration { get; set; }

            public sealed class Connection {
                
                public string Endpoint { get; set; }
                
                public string Port { get; set; }
                
                public string Password { get; set; }
                
                public string Ssl { get; set; }
                
                public string DefaultDb { get; set; }
                
                public string AbortConnect { get; set; }
                
                public string AllowAdmin { get; set; }
                
                public string InstanceName { get; set; }
            }
        }
    }

    public sealed class Staging: Development { }

    public sealed class Production: Development { }
}
