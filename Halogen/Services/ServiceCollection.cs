using Autofac;
using Halogen.DbContexts;
using Halogen.Services.AppServices.Interfaces;
using Halogen.Services.AppServices.Services;
using Halogen.Services.DbServices.Interfaces;
using Halogen.Services.DbServices.Services;

namespace Halogen.Services;

public static class ServiceCollection {
    
    public static void RegisterHalogenServices(this ContainerBuilder builder) {
        builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().SingleInstance();
        builder.RegisterType<HalogenDbContext>();
        builder.RegisterType<RedisCache>().As<ICacheService>();
        builder.RegisterType<JwtService>().As<IJwtService>();
        builder.RegisterType<SessionService>().As<ISessionService>();
        
        builder.RegisterType<ContextService>().As<IContextService>();
        builder.RegisterType<AccountService>().As<IAccountService>();
        builder.RegisterType<AddressService>().As<IAddressService>();
        builder.RegisterType<AuthenticationService>().As<IAuthenticationService>();
        builder.RegisterType<ChallengeService>().As<IChallengeService>();
        builder.RegisterType<LocalityService>().As<ILocalityService>();
        builder.RegisterType<PreferenceService>().As<IPreferenceService>();
        builder.RegisterType<ProfileService>().As<IProfileService>();
        builder.RegisterType<RoleService>().As<IRoleService>();
        builder.RegisterType<TrustedDeviceService>().As<ITrustedDeviceService>();
    }
}
