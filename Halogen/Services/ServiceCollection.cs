using Autofac;
using Halogen.DbContexts;
using Halogen.FactoriesAndMiddlewares;
using Halogen.FactoriesAndMiddlewares.Interfaces;
using HelperLibrary.Shared.Logger;

namespace Halogen.Services;

public static class ServiceCollection {
    
    public static void RegisterHalogenServices(this ContainerBuilder builder) {
        builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().SingleInstance();
        builder.RegisterType<HalogenDbContext>().SingleInstance();
        builder.RegisterType<LoggerService>().As<ILoggerService>().SingleInstance();
        
        builder.RegisterType<HaloServiceFactory>().As<IHaloServiceFactory>().SingleInstance();
    }
}
