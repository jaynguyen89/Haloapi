using Autofac;
using Halogen.DbContexts;
using Halogen.Services.CacheServices.Interfaces;
using Halogen.Services.CacheServices.Services;
using Halogen.Services.DbServices.Interfaces;
using Halogen.Services.DbServices.Services;

namespace Halogen.Services;

public static class ServiceInjections {
    
    public static void RegisterHalogenServices(this ContainerBuilder builder) {
        builder.RegisterType<HalogenDbContext>();
        builder.RegisterType<RedisCache>().As<ICacheService>();
        builder.RegisterType<ContextService>().As<IContextService>();
        builder.RegisterType<IJwtService>().As<JwtService>();
    }
}
